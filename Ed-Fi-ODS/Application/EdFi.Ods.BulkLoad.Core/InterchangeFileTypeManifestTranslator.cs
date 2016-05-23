using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using NHibernate.Linq;

namespace EdFi.Ods.BulkLoad.Core
{
    public class InterchangeFileTypeManifestTranslator : IInterchangeFileTypeTranslator
    {
        private Dictionary<string, string> _interchangeFileDictionary;
        private readonly string _manifestFile;

        public InterchangeFileTypeManifestTranslator(string manifestPath)
        {
            _manifestFile = manifestPath;
        }

        private Dictionary<string, string> InterchangeFileDictionary
        {
            get { return _interchangeFileDictionary ?? (_interchangeFileDictionary = LoadFromManifest()); }
        }

        private Dictionary<string, string> LoadFromManifest()
        {
            if (!File.Exists(_manifestFile))
                throw new Exception("Could not load manifest file. File not found.");

            var doc = XElement.Load(_manifestFile);
            var files = new Dictionary<string, string>();
            doc.Descendants("Interchange")
                .ForEach(x =>
                {
                    var filename = x.Element("Filename");
                    var type = x.Element("Type");
                    if (filename == null || type == null)
                        throw new Exception(string.Format("Could not load interchange manifest: {0}", _manifestFile));
                    files.Add(filename.Value, type.Value);
                });

            return files;
        }

        public string GetInterchangeType(string fileName)
        {
            return !InterchangeFileDictionary.ContainsKey(fileName) ? string.Empty : InterchangeFileDictionary[fileName];
        }
    }
}
