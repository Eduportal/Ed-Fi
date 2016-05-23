using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace EdFi.Ods.Api.Services.Metadata
{
    /// <summary>
    /// Provides access to cached Swagger metadata, accessible by the resource's "file" name.
    /// </summary>
    internal class SwaggerMetadataCache
    {
        private static readonly Lazy<IDictionary<string, string>> _metadataByResourceItemName;
        private static readonly Lazy<int> _swaggerMetadataHash;

        static SwaggerMetadataCache()
        {
            _metadataByResourceItemName = new Lazy<IDictionary<string, string>>(LoadAllSwaggerMetadata);
            _swaggerMetadataHash = new Lazy<int>(ComputeSwaggerMetadataHash);
        }

        /// <summary>
        /// Gets the computed hash for the Swagger metadata content (useful in detecting changes).
        /// </summary>
        public static int SwaggerMetadataHash
        {
            get { return _swaggerMetadataHash.Value; }
        }
        
        private static int ComputeSwaggerMetadataHash()
        {
            return ComputeHash(GetSwaggerBytes());
        }

        private static IEnumerable<byte> GetSwaggerBytes()
        {
            foreach (string key in MetadataByResourceItemName.Keys.OrderBy(x => x))
            {
                foreach (byte b in Encoding.UTF8.GetBytes(MetadataByResourceItemName[key]))
                    yield return b;
            }
        }

        private static int ComputeHash(IEnumerable<byte> data)
        {
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                foreach (byte b in data)
                    hash = (hash ^ b) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }

        /// <summary>
        /// Gets a dictionary containing the Swagger metadata keyed by resource's "file" name 
        /// (e.g. resources.api-docs.json, resources.academicWeeks.json, etc.).
        /// </summary>
        public static IDictionary<string, string> MetadataByResourceItemName
        {
            get { return _metadataByResourceItemName.Value; }
        }

        private static IDictionary<string, string> LoadAllSwaggerMetadata()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var metadataSources =
                (assemblies.SelectMany(assembly => 
                    CustomAttributeExtensions.GetCustomAttributes(assembly, typeof(SwaggerMetadataResourceAttribute))
                     .Cast<SwaggerMetadataResourceAttribute>()
                     .Select(attr => new { Assembly = assembly, BaseName = attr.BaseName })
                    ))
                    .ToList();

            var catalog = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // Example embedded resource entries:
            //  resources.api-docs.json
            //  resources.academicWeeks.json
            //  Test-Profile-Resource-IncludeOnly.api-docs.json
            //  Test-Profile-Resource-IncludeOnly.schools.json

            foreach (var metadataSource in metadataSources)
            {
                var resourceManager = new ResourceManager(metadataSource.BaseName, metadataSource.Assembly);

                var resourceSet = resourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);

                var enumerator = resourceSet.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    if (enumerator.Value is string)
                    {
                        catalog.Add(
                            enumerator.Key.ToString(),
                            enumerator.Value.ToString());
                    }
                }
            }

            return catalog;
        }
    }
}