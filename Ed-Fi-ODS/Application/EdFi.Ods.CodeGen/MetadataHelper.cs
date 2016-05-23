using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using EdFi.Ods.CodeGen.Models.ProfileMetadata;

namespace EdFi.Ods.CodeGen
{
    public class MetadataHelper
    {
        public static XDocument GetDomainMetadata(string extensionDomainMetadataPath)
        {
            using (var stream = new MemoryStream())
            {
                // make a in-memory copy of the resource stream to avoid locking during build
                using (
                    var resStream =
                        Assembly.GetExecutingAssembly()
                                .GetManifestResourceStream(
                                    "EdFi.Ods.CodeGen.App_Packages.Ed_Fi.Metadata.DomainMetadata.xml"))
                {
                    resStream.CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    resStream.Close();
                }
                var mainDoc = XDocument.Load(stream);
                var mainDocAggregates = mainDoc.Element("Aggregates");

                var extensionFilename = Path.Combine(extensionDomainMetadataPath, "DomainMetadata-Extension.xml");
                if (File.Exists(extensionFilename))
                {
                    var extensionDoc = XDocument.Load(extensionFilename);

                    // Process aggregate extensions first
                    var aggregateExtensions = extensionDoc.Descendants("AggregateExtension");

                    foreach (var aggregateExtension in aggregateExtensions)
                    {
                        string rootTableName = aggregateExtension.AttributeValue("root");

                        // Find the corresponding Aggregate in the mainDoc
                        var targetAggregate =
                            mainDoc.Descendants("Aggregate")
                                   .SingleOrDefault(e => XElementExtensions.AttributeValue(e, "root").Equals(rootTableName));

                        if (targetAggregate == null)
                            throw new Exception(
                                String.Format(
                                    "Unable to find target aggregate with root '{0}'.  Cannot apply aggregate extension metadata.",
                                    rootTableName));

                        targetAggregate.Add(
                            from e in aggregateExtension.Elements("Entity")
                            select
                                new XElement(
                                    "Entity",
                                    new XAttribute("table", e.AttributeValue("table")),
                                    new XAttribute("schema", "extension")));
                    }

                    // Now merge/replace the aggregates defined in the extension metadata
                    var extensionAggregates = extensionDoc.Descendants("Aggregate");

                    foreach (var extensionAggregate in extensionAggregates)
                    {
                        string rootTableName = extensionAggregate.AttributeValue("root");

                        var existingAggregate =
                            mainDoc.Descendants("Aggregate")
                                   .SingleOrDefault(e => e.AttributeValue("root").Equals(rootTableName));

                        if (existingAggregate != null) existingAggregate.Remove();

                        mainDocAggregates.Add(extensionAggregate);
                    }
                }
                return mainDoc;
            }
        }

        public static XDocument GetProfilesXDocument(string profilesMetadataPath)
        {
            string xml = GetProfilesRawXml(profilesMetadataPath);

            if (xml == null)
                return new XDocument();

            return XDocument.Parse(xml);
        }

        public static Profiles GetProfiles(string profilesMetadataPath)
        {
            string xml = GetProfilesRawXml(profilesMetadataPath);

            if (xml == null)
                return new Profiles { Profile = new Profile[0]};
            
            ValidateRawXml(xml);
            var sr = new StringReader(xml);
            var serializer = new XmlSerializer(typeof(Profiles));
            var profiles = (Profiles)serializer.Deserialize(sr);

            return profiles;

            #region Commented out, original implementation left for possible future reference

            // This approach scanned all XML files in the path provided, assembling all Profiles
            // Subsequent development assumes the file to be a single file named "Profiles.xml",
            // but this behavior may be desirable, so it is being left here for possible future inclusion.

            // Find all Profiles documents in the specified folder
            //var profiles = new Profiles { Profile = new Profile[0] };

            //string[] possibleProfileDocuments = Directory.GetFiles(profilesMetadataPath, "Profiles.xml");

            //foreach (string document in possibleProfileDocuments)
            //{
            //    string xml = File.ReadAllText(document);

            //    if (xml.Contains("<Profiles>"))
            //    {
            //        var sr = new StringReader(xml);
            //        var fileProfiles = (Profiles)serializer.Deserialize(sr);

            //        // Add these profiles to the cumulative list
            //        profiles.Profile = profiles.Profile.Concat(fileProfiles.Profile).ToArray();
            //    }
            //}

            //return profiles;

            #endregion
        }

        private static string GetProfilesRawXml(string profilesMetadataPath)
        {
            // By convention, this should only look for a file called "Profiles.xml"
            string profileFileName = Path.Combine(profilesMetadataPath, "Profiles.xml");

            if (!File.Exists(profileFileName))
                return null;
            
            string xml = File.ReadAllText(profileFileName);
            return xml;
        }

        private static void ValidateRawXml(string rawXml)
        {
            var xmlReaderSettings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema
            };
            xmlReaderSettings.ValidationEventHandler += (sender, args) =>
            {
                throw new Exception(string.Format("The profiles specified are not valid. {0} on line number {1} line position {2}", args.Message, args.Exception.LineNumber,
                    args.Exception.LinePosition));
            };

            XmlTextReader profilesXsd;
            var currentAssembly = Assembly.GetExecutingAssembly();
            var resourceName = @"EdFi.Ods.CodeGen.Models.ProfileMetadata.Ed-Fi-ODS-API-Profiles.xsd";
            using (Stream stream = currentAssembly.GetManifestResourceStream(resourceName))
            {
                profilesXsd = new XmlTextReader(stream);
            }
            
            xmlReaderSettings.Schemas.Add("", profilesXsd);
            var profilesXml = new XmlTextReader(new StringReader(rawXml));
            
            var xmlReader = XmlReader.Create(profilesXml, xmlReaderSettings);

            while (xmlReader.Read()) ;
        }

        public static bool IsProfileReadOnly(Profile profile, string resourceName)
        {
            var targetProfileResource = profile.Resource.First(resource => resource.name == resourceName);
            return (targetProfileResource.WriteContentType == null);
        }

        public static bool IsProfileWriteOnly(Profile profile, string resourceName)
        {
            var targetProfileResource = profile.Resource.First(resource => resource.name == resourceName);
            return (targetProfileResource.ReadContentType == null);
        }
    }
}