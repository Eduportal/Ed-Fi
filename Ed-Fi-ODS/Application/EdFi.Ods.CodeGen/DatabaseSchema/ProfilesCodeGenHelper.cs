// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using EdFi.Common.Extensions;
using EdFi.Common.Inflection;
using EdFi.Ods.CodeGen.Models;
using EdFi.Ods.CodeGen.Models.ProfileMetadata;
using EdFi.Ods.Common.Utils.Profiles;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public class ProfileContext
    {
        public Profile Profile { get; set; }
        public XElement ProfileXElement { get; set; }
        public ContentTypeUsage? ContentTypeUsage { get; set; }
    }

    public class ProfilesCodeGenHelper : CodeGenHelperDecoratorBase
    {
        private readonly ProfileContext _profileContext;
        private readonly XmlDocument _profileXmlDocument = new XmlDocument();
        private XDocument _domainMetadataXDocument = new XDocument();
        
        public ProfilesCodeGenHelper(ProfileContext profileContext, ICodeGenHelper decoratedHelper)
            : base(decoratedHelper)
        {
            _profileContext = profileContext;

            var xmlSerializer = new XmlSerializer(typeof (Profile));
            var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, _profileContext.Profile);
            _profileXmlDocument.LoadXml(stringWriter.ToString());
        }

        /// <summary>
        /// Indicates that there are multiple contexts in which a particular aggregate should be generated.
        /// </summary>
        /// <param name="aggregateRootName">The name of the aggregate to be generated.</param>
        /// <returns>A list containing the context values to be used in multiple generation calls by the 
        /// code generation template.</returns>
        public override List<string> GetGenerationContexts(string aggregateRootName)
        {
            if (_profileContext == null)
                return base.GetGenerationContexts(aggregateRootName);

            var aggregateRootClassModel = DomainModel.GetClassModel(aggregateRootName);

            // If the aggregate is not an abstract base class, there are no special contexts to provide
            if (!aggregateRootClassModel.IsBaseTable) // || !aggregateRootClassModel.IsAbstract)
                return base.GetGenerationContexts(aggregateRootName);

            // Return the names of resources in the current profile that are derived from this abstract aggregate root
            var derivedResourceNames = GetDerivedResourcesInProfile(aggregateRootClassModel)
                .Select(r => r.name)
                .ToList();

            return derivedResourceNames;
        }

        public override bool ShouldGenerateAggregate(string aggregateRootName, string context)
        {
            var aggregateRootClassModel = DomainModel.GetClassModel(aggregateRootName);

            // If the aggregate root is an abstract base class of a resource included in the Profile,
            // then we need to go ahead and generate its contents so that the profile-specific
            // resource definition (with possible constraints placed on child collections) has a place 
            // to be manifested.
            if (aggregateRootClassModel.IsBaseTable
                && GetDerivedResourcesInProfile(aggregateRootClassModel)
                    .Any(ProfileDefinitionHasResourceContentTypeForCurrentProfileContext))
            {
                return true;
            }

            // Only generate the aggregate if it is explicitly present in the Profile definition and has
            // a ContentType defined matching the current profile context
            if (_profileContext.Profile.Resource
                .Any(r => r.name.EqualsIgnoreCase(aggregateRootName)
                            && ProfileDefinitionHasResourceContentTypeForCurrentProfileContext(r)))
            {
                return true;
            }

            return false;
        }

        private bool ProfileDefinitionHasResourceContentTypeForCurrentProfileContext(Resource resource)
        {
            return (
                _profileContext.ContentTypeUsage == null
                || (_profileContext.ContentTypeUsage == ContentTypeUsage.Readable && resource.ReadContentType != null)
                || (_profileContext.ContentTypeUsage == ContentTypeUsage.Writable && resource.WriteContentType != null));
        }

        public override bool ShouldGenerateEntity(string aggregateRootName, string entityName, string context)
        {
            if (entityName == aggregateRootName)
            {
                var aggregateRootClassModel = DomainModel.GetClassModel(aggregateRootName);

                // Don't generate entities that are abstract base classes
                if (aggregateRootClassModel.IsBaseTable)
                    return false;
            }

            return base.ShouldGenerateEntity(aggregateRootName, entityName, context);
        }

        private List<Resource> GetDerivedResourcesInProfile(CodeGenClassModel aggregateRootClassModel)
        {
            var derivedTableNames = aggregateRootClassModel.DerivedTables.Select(dt => dt.Name).ToList();
            var profileResources = _profileContext.Profile.Resource.ToList();

            var derivedResourcesInProfile = profileResources
                .Where(r => derivedTableNames.Contains(r.name, StringComparer.InvariantCultureIgnoreCase))
                .ToList();

            return derivedResourcesInProfile;
        }

        private bool ShouldReframeQuestionToCurrentContext(string entityName, string context)
        {
            var entityClassModel = DomainModel.GetClassModel(entityName);

            if (entityClassModel.IsBaseTable)
            {
                var derivedResourcesInProfile = GetDerivedResourcesInProfile(entityClassModel);

                // If we have a derived resource in the profile matching the supplied context
                return (derivedResourcesInProfile.Any(r => r.name.EqualsIgnoreCase(context)));
            }

            return false;
        }

        public override bool ShouldGenerateMember(string aggregateRootName, string entityName, string memberName, string context)
        {
            if (ShouldReframeQuestionToCurrentContext(aggregateRootName, context))
            {
                // ... then reframe the question in terms of the derived table/resource
                return ShouldGenerateMember(
                    context,
                    entityName,
                    memberName, context);
            }

            // Is the property a member of a foreign key on the entity/table?
            var table = GetTable(entityName);
            
            // Find all incoming FKs which include the specified member, but aren't direct references to lookup tables (types, descriptors)
            var incomingFks = table.FKTables
                .Where(fkt => !fkt.IsPrimaryTable 
                    && fkt.ThisColumns.Contains(memberName)
                    && !IsDirectLookupReference((Dictionary<string, Table>) TablesByName, fkt))
                .ToList();

            if (incomingFks.Any())
            {
                // Determine whether the property should be generated based on the references that it is used in.
                return incomingFks.Any(fkt => ShouldGenerateReference(aggregateRootName, entityName, fkt, context));
            }

            string profileMemberName = TrimIdSuffixesFromTypesAndDescriptors(memberName);

            return ShouldGenerateProfileMember(aggregateRootName, entityName, profileMemberName, context);
        }

        private string TrimIdSuffixesFromTypesAndDescriptors(string memberName)
        {
            if (memberName.EndsWith("TypeId") || memberName.EndsWith("DescriptorId"))
                return memberName.TrimSuffix("Id");

            return memberName;
        }
        
        /// <summary>
        /// Determines whether to generate a member based on the supplied profile member name.
        /// </summary>
        /// <param name="aggregateRootName">The name of the aggregate root.</param>
        /// <param name="entityName">The name of the current entity.</param>
        /// <param name="memberName">The member name that has been converted to text that would appear in a profile definition.</param>
        /// <returns><b>true</b> if the member should be generated; otherwise <b>false</b>;</returns>
        private bool ShouldGenerateProfileMember(string aggregateRootName, string entityName, string memberName, string context, bool isRecursiveCall = false)
        {
            var resource = _profileContext.Profile
                .Resource
                .SingleOrDefault(x => x.name.EqualsIgnoreCase(aggregateRootName));
            
            if (resource == null)
                throw new Exception(string.Format("Resource '{0}' not found in profile '{1}' (while requesting member '{2}' of entity '{3}').", 
                    aggregateRootName, _profileContext.Profile.name, memberName, entityName));

            
            ContentType contentType = null;

            if (_profileContext.ContentTypeUsage == ContentTypeUsage.Readable)
            {
                contentType = resource.ReadContentType;
            }
            else if (_profileContext.ContentTypeUsage == ContentTypeUsage.Writable)
            {
                contentType = resource.WriteContentType;
            }
            else
            {
                throw new Exception(
                    "Profile context does not have a content type usage assigned which is required for determining whether to generate individual members.");
            }

            if (contentType == null)
                return false;

            if (contentType.memberSelection == MemberSelectionMode.IncludeAll)
                return true;

            if (contentType.memberSelection == MemberSelectionMode.ExcludeAll)
                return false;

            var memberInfo = GetMemberInfo(aggregateRootName, entityName, memberName, _profileContext.ContentTypeUsage);

            Func<MemberSelectionMode,bool> evaluateSpecifiedMemberSelection = (memberSelectionMode) =>
            {
                return memberSelectionMode == MemberSelectionMode.IncludeOnly;
            };

            Func<MemberSelectionMode,bool> evaluateUnspecifiedMemberSelection = (memberSelectionMode) =>
            {
                return memberSelectionMode != MemberSelectionMode.IncludeOnly;
            };

            if (memberInfo.XPathMatches)
            {
                SetMemberSelectionMode(memberInfo,isRecursiveCall);

                if (memberInfo.MemberSelectionMode == MemberSelectionMode.IncludeAll)
                {
                    return true;
                }
                if (memberInfo.MemberSelectionMode == MemberSelectionMode.ExcludeAll)
                {
                    return false;
                }

                if (!isRecursiveCall)
                {
                    return evaluateSpecifiedMemberSelection(memberInfo.MemberSelectionMode); ;
                }
                return evaluateUnspecifiedMemberSelection(memberInfo.MemberSelectionMode);
            }

            // xPath failed to match through profile.
            if (entityName == aggregateRootName)
                return evaluateUnspecifiedMemberSelection(contentType.memberSelection);
            
            var currentEntityClassModel = DomainModel.GetClassModel(entityName);

            var currentEntityParentName = currentEntityClassModel.ParentFkTable.OtherTable;
            var parentClassModel = DomainModel.GetClassModel(currentEntityParentName);

            var isOneToOneRelationship = false;
            if (parentClassModel.HasAnyOneToOneChildFKs)
            {
                isOneToOneRelationship = parentClassModel.OneToOneChildFKs.Any(FKTable => FKTable.OtherTable == entityName);
            }

            return ShouldGenerateProfileMember(aggregateRootName, 
                ShouldReframeQuestionToCurrentContext(currentEntityParentName, context) ? context : currentEntityParentName,
                isOneToOneRelationship ? entityName : CompositeTermInflector.MakePlural(entityName), context, true);
        }

        internal class MemberInfo
        {
            public string XPath { get; set; }
            public bool XPathMatches { get; set; }
            public MemberSelectionMode MemberSelectionMode { get; set; }
        }

        private MemberInfo GetMemberInfo(string aggregateRootName, string entityName, string memberName, 
            ContentTypeUsage? contentTypeUsage)
        {
            if (contentTypeUsage == null)
            {
                throw new ArgumentNullException("contentTypeUsage", 
                    string.Format("ContentTypeUsage must be provided in this context."));
            }

            string contentTypeUsageElementName = GetContentTypeUsageElementName(contentTypeUsage);

            string contentTypeXPath = string.Format(
                "//Resource[@name='{0}']/{1}", 
                aggregateRootName, contentTypeUsageElementName);

            // If the entity we're looking for is the top-level aggregate root / resource
            if (entityName == aggregateRootName)
            {
                var xPathTopLevel = string.Format(
                    "{0}/Property[@name='{1}']|{0}/Object[@name='{1}']|{0}/Collection[@name='{1}']",
                    contentTypeXPath, memberName);

                return new MemberInfo
                {
                    XPath = xPathTopLevel,
                    XPathMatches = (_profileXmlDocument.SelectSingleNode(xPathTopLevel) != null)
                };
            }

            // Not the top-level resource, so look for a Collection named appropriately for the entity
            var xPathEmbedded = string.Format(
                "{0}//Collection[@name='{1}']/Property[@name='{3}']" +
                "|{0}//Collection[@name='{1}']/Object[@name='{3}']" +
                "|{0}//Collection[@name='{1}']/Collection[@name='{3}']" +
                "|{0}//Object[@name='{2}']/Property[@name='{3}']" +
                "|{0}//Object[@name='{2}']/Object[@name='{3}']" +
                "|{0}//Object[@name='{2}']/Collection[@name='{3}']",
                contentTypeXPath, CompositeTermInflector.MakePlural(entityName), entityName, memberName);
            return new MemberInfo
            {
                XPath = xPathEmbedded,
                XPathMatches = (_profileXmlDocument.SelectSingleNode(xPathEmbedded) != null)
            };
        }

        private void SetMemberSelectionMode(MemberInfo memberInfo, bool isRecursiveCall)
        {
            // If we get here with recursion, it means we didnt find the initial member, 
            // so we need to find the memberSelection on the new, current member.
            // Otherwise, we want the member selection of the containing parent node.
            var nodeToSearch = (isRecursiveCall) ? 
                _profileXmlDocument.SelectSingleNode(memberInfo.XPath) 
                : _profileXmlDocument.SelectSingleNode(memberInfo.XPath).ParentNode;

            var memberSelectionAttribute = nodeToSearch.Attributes.Cast<XmlAttribute>()
                                   .Where(attr => attr.Name == "memberSelection")
                                   .Select(attribute => attribute.Value)
                                   .First();
                       
            MemberSelectionMode selectionMode;
            Enum.TryParse(memberSelectionAttribute, out selectionMode);
            memberInfo.MemberSelectionMode = selectionMode;
        }

        private static string GetContentTypeUsageElementName(ContentTypeUsage? contentTypeUsage)
        {
            switch (contentTypeUsage)
            {
                case ContentTypeUsage.Readable:
                    return "ReadContentType";

                case ContentTypeUsage.Writable:
                    return "WriteContentType";

                default:
                    throw new NotSupportedException(string.Format(
                        "Unexpected ContentTypeUsage of '{0}' was encountered.", 
                        contentTypeUsage));
            }
        }

        public override bool ShouldGenerateReference(string aggregateRootName, string entityName, FKTable incomingFK, string context)
        {
            // If the FK is the exclusive source of any of the table's PK values, we must include it.
            if (IsIncomingFKExclusiveSourceForAnyPKColumns(incomingFK) && !incomingFK.IsPrimaryTable)
                return true;
            
            string targetAggregateRootName = 
                ShouldReframeReferenceToSuppliedContext(aggregateRootName, context) 
                    ? context
                    : aggregateRootName;
                    
            var thisTable = GetTable(incomingFK.ThisTable);
            bool isOneToOneRelationship =
                thisTable.GetNavigableOneToOneChildTables((Dictionary<string, Table>) TablesByName).Contains(incomingFK);
            
            // Build the member name as it should appear in the profile definition
            string profileMemberName = GetRoleName(incomingFK) + incomingFK.OtherClass + 
                (isOneToOneRelationship ? string.Empty : "Reference") ;

            return ShouldGenerateProfileMember(targetAggregateRootName, entityName, profileMemberName, context);
        }

        private bool ShouldReframeReferenceToSuppliedContext(string aggregateRootName, string context)
        {
            var aggregateRootClassModel = DomainModel.GetClassModel(aggregateRootName);

            if (aggregateRootClassModel.IsBaseTable) // && aggregateRootClassModel.IsAbstract)
            {
                var derivedResourcesInProfile = GetDerivedResourcesInProfile(aggregateRootClassModel);

                // If we have a derived resource in the profile, then reframe the query in terms of the derived table/resource
                if (derivedResourcesInProfile.Any(r => r.name.EqualsIgnoreCase(context)))
                    return true;
            }

            return false;
        }

        private bool IsIncomingFKExclusiveSourceForAnyPKColumns(FKTable incomingFK)
        {
            var thisTable = GetTable(incomingFK.ThisTable);

            // Get a list of the columns in this reference that are part of the PK
            var contributedPKColumns =
                incomingFK.ThisColumns
                          .Intersect(thisTable.PKs.Select(pk => pk.Name))
                          .ToList();

            // Does this reference contribute PK values?
            if (contributedPKColumns.Any())
            {
                // Determine if this FK is the exclusive source for any of its contributed PK columns
                bool isExclusiveSource =
                    contributedPKColumns.Any(
                        cn =>
                            thisTable.ParentTables
                                // Don't include the FK we're processing
                                .Except(new[] {incomingFK})
                                // Do all the other FKs NOT have this column? (the FK is the exclusive source)
                                .All(fkt => !fkt.ThisColumns.Any(x => x == cn))
                        );

                // If the FK is the exclusive source of any of the table's PK values, we must include it.
                if (isExclusiveSource)
                    return true;
            }

            return false;
        }
    }
}