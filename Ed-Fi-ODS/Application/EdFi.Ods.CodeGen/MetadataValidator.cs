using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using EdFi.Common.Extensions;
using EdFi.Common.Inflection;
using EdFi.Ods.CodeGen.DatabaseSchema;
using EdFi.Ods.CodeGen.Models;
using EdFi.Ods.CodeGen.Models.ProfileMetadata;
using EdFi.Ods.Common.Utils.Extensions;

namespace EdFi.Ods.CodeGen
{
    public class MetadataValidator
    {
        private static XDocument _profileXDocument;
        private static List<string> _profileIssues = new List<string>();

        public static void ValidateProfileMetadata(ICodeGenHelper codeGenHelper,
            Profiles profiles, XDocument profileXDocument)
        {
            _profileXDocument = profileXDocument;

            ProfileNamesAreUnique(profiles);
            profiles.Profile.ForEach(profile => ProfileIsValid((CodeGenHelper) codeGenHelper, profile));

            if (_profileIssues.Any())
            {
                throw new Exception(string.Join(Environment.NewLine,_profileIssues.ToArray()));
            }
        }

        private static void ProfileNamesAreUnique(Profiles profiles)
        {
            var sortedProfileNames = profiles.Profile.OrderBy(profile => profile.name).Select(profile => profile.name).ToList();
            if (sortedProfileNames.Distinct().Count() != sortedProfileNames.Count())
            {
                _profileIssues.Add("There are duplicate profiles detected in Profiles.xml");
            }
        }

        private static void ProfileIsValid(CodeGenHelper codeGenHelper, Profile profile)
        {
            foreach (Resource currentResource in profile.Resource)
            {
                var resourceName = currentResource.name;
                var resourceClassModel = codeGenHelper.DomainModel.GetClassModel(currentResource.name);
                if (resourceClassModel == null)
                {
                    return;
                }
                var resourceHasBaseClass = (resourceClassModel.BaseClass != null);

                ValidateProperties(codeGenHelper, profile, resourceName, resourceClassModel, resourceHasBaseClass);
                ValidateObjects(profile, resourceName, resourceClassModel, resourceHasBaseClass);
                ValidateCollections(profile, resourceName, resourceClassModel, resourceHasBaseClass);
            }
        }

        private static void ValidateCollections(Profile profile, string resourceName, CodeGenClassModel resourceClassModel, 
            bool resourceHasBaseClass)
        {
            var resourceCollectionsXPath = CreateMemberXPath(profile.name, resourceName, "Collection");
            var resourceCollectionElements = GetXElementsFromXPath(resourceCollectionsXPath);
            if (!resourceCollectionElements.Any())
            {
                return;
            }

            foreach (var collectionElement in resourceCollectionElements)
            {
                var collectionName = collectionElement.Attribute("name").Value;
                var containingTable = (collectionElement.Parent.Attribute("name") != null)
                    ? collectionElement.Parent.Attribute("name").Value
                    : resourceName;
               
                var resourceCollectionIsValid = resourceClassModel.AggregateTableNames.Any(name => name != resourceName
                    && name == CompositeTermInflector.MakeSingular(collectionName)) || 
                    ((resourceHasBaseClass) && resourceClassModel.BaseClass.AggregateTableNames.Any(baseName =>
                                                                  baseName == CompositeTermInflector.MakeSingular(collectionName)));
                if (!resourceCollectionIsValid)
                {
                    _profileIssues.Add(string.Format(
                        "Collection property '{0}' contained in Profile '{1}' " +
                        "did not have a corresponding foreign key on table '{2}'.",
                        collectionName, profile.name, containingTable));
                }
            }
        }

        private static void ValidateObjects(Profile profile, string resourceName, CodeGenClassModel resourceClassModel, 
            bool resourceHasBaseClass)
        {
            var resourceObjectsXPath = CreateMemberXPath(profile.name, resourceName, "Object");
            var resourceObjectsElements = GetXElementsFromXPath(resourceObjectsXPath);

            if (!resourceObjectsElements.Any())
            {
                return;
            }

            foreach (var objectElement in resourceObjectsElements)
            {
                var objectName = objectElement.Attribute("name").Value;
                var containingTable = (objectElement.Parent.Attribute("name") != null)
                    ? objectElement.Parent.Attribute("name").Value
                    : resourceName;

                var resourceObjectIsValid = resourceClassModel.OneToOneChildFKs.Any(table =>
                    table.IsPrimaryTable
                    && table.OtherTable == objectName) ||
                                            ((resourceHasBaseClass) &&
                                             resourceClassModel.BaseClass.AggregateTableNames.Any(baseName =>
                                                 baseName == objectName));
                if (!resourceObjectIsValid)
                {
                    _profileIssues.Add(string.Format(
                        "Object property '{0}' contained in Profile '{1}' " +
                        "did not have a corresponding foreign key on table '{2}'.",
                        objectName, profile.name, containingTable));
                }
            }
        }

        private static void ValidateProperties(CodeGenHelper codeGenHelper, Profile profile, string resourceName, 
            CodeGenClassModel resourceClassModel, bool resourceHasBaseClass)
        {
            var resourcePropertiesXPath = CreateMemberXPath(profile.name, resourceName, "Property");
            var resourcePropertyElements = GetXElementsFromXPath(resourcePropertiesXPath);
            if (!resourcePropertyElements.Any())
            {
                return;
            }

            foreach (var propertyElement in resourcePropertyElements)
            {
                var propertyName = propertyElement.Attribute("name").Value;
                var containingTable = (propertyElement.Parent.Attribute("name") != null)
                    ? propertyElement.Parent.Attribute("name").Value
                    : resourceName;
                if (propertyName.EndsWith("Reference"))
                {
                    Func<string, Table, List<string>, bool> ValidatePropertyReference =
                        (pName, underlyingTable, aggregateTableNames) =>
                        {
                            var propertyToSearch =
                                pName.Remove(pName.IndexOf("Reference", StringComparison.Ordinal));

                            // Retrieve the FK relations for the resource. Then check for a match on the property.
                            var inboundFKRelations =
                                codeGenHelper.GetNonNavigableParentRelationships(underlyingTable,
                                    aggregateTableNames);
                            if (inboundFKRelations.Any(fkt => fkt.OtherTable == propertyToSearch))
                            {
                                return true;
                            }

                            // Find all possible role specific relations.
                            var possibleRoleNamedRelations = inboundFKRelations.Where(fkt =>
                                propertyToSearch.EndsWith(fkt.OtherTable));

                            // Find the exact match of the FK role relation.
                            if (possibleRoleNamedRelations.Any(fkt =>
                                codeGenHelper.GetRoleName(fkt) == propertyToSearch.TrimSuffix(fkt.OtherTable)))
                            {
                                return true;
                            }

                            return false;
                        };

                    var resourcePropertyReferenceIsValid = ValidatePropertyReference(propertyName,
                        resourceClassModel.UnderlyingTable,
                        resourceClassModel.AggregateTableNames);
                    if (!resourcePropertyReferenceIsValid)
                    {
                        if (!resourceHasBaseClass)
                        {
                            _profileIssues.Add(string.Format(
                                "The Reference property '{0}' contained in Profile '{1}' " +
                                "did not match an incoming foreign key relationship on table '{2}'.",
                                propertyName, profile.name, containingTable));
                        }
                        else
                        {
                            if (!ValidatePropertyReference(propertyName,
                                resourceClassModel.BaseClass.UnderlyingTable,
                                resourceClassModel.BaseClass.AggregateTableNames))
                            {
                                _profileIssues.Add(string.Format(
                                    "The Reference property '{0}' contained in Profile '{1}' " +
                                    "did not match an incoming foreign key relationship on table '{2}'.",
                                    propertyName, profile.name, containingTable));
                            }
                        }
                    }
                }

                if (propertyName.EndsWith("Type") || propertyName.EndsWith("Descriptor"))
                {
                    var entityPropertyNameSearch = propertyName + "Id";
                    var columnMatchingProperty = resourceClassModel.NonPrimaryKeyColumns.FirstOrDefault(
                        column => column.Name == entityPropertyNameSearch);
                    if (resourceHasBaseClass)
                    {
                        columnMatchingProperty = resourceClassModel.BaseClass.NonPrimaryKeyColumns
                            .FirstOrDefault(
                                column => column.Name == entityPropertyNameSearch);
                    }

                    if (columnMatchingProperty != null)
                    {
                        if (columnMatchingProperty.IsPK)
                        {
                            _profileIssues.Add(string.Format(
                                "The property '{0}' contained in Profile '{1}' cannot be included in the Profile definition" +
                                " because it is part of the '{2}' table's primary key.  Primary keys must always be included.",
                                propertyName, profile.name, containingTable));
                        }

                        if (columnMatchingProperty.IsForeignKey)
                        {
                            var columnTable = codeGenHelper.GetTable(columnMatchingProperty.TableName);
                            var tableContainingMatchingColumn = new FKTable();
                            foreach (var fkt in columnTable.FKTables)
                            {
                                if (fkt.ThisColumns.FirstOrDefault(column => column == columnMatchingProperty.Name) !=
                                    null)
                                {
                                    tableContainingMatchingColumn = fkt;
                                    break;
                                }
                            }

                            if (!codeGenHelper.IsDirectLookupReference((Dictionary<string, Table>)codeGenHelper.TablesByName, tableContainingMatchingColumn))
                            {
                                _profileIssues.Add(string.Format(
                                "The property '{0}' contained in Profile '{1}' cannot be included in the Profile definition" +
                                " because it is part of an incoming FK reference from '{2}' to '{3}'. Reference property instead.",
                                propertyName, profile.name, containingTable, tableContainingMatchingColumn.OtherTable));
                            }
                        }
                    }
                }
            }
        }

        private static string CreateMemberXPath(string profileName, string resourceName, string memberType)
        {
            return string.Format("//Profile[@name='{0}']//Resource[@name='{1}']/{2}//{4}|//Profile[@name='{0}']//Resource[@name='{1}']/{3}//{4}",
                        profileName, resourceName, "ReadContentType", "WriteContentType",memberType);
        }

        private static List<XElement> GetXElementsFromXPath(string xPath)
        {
           return _profileXDocument.XPathSelectElements(xPath).ToList();
        }
    }
}
