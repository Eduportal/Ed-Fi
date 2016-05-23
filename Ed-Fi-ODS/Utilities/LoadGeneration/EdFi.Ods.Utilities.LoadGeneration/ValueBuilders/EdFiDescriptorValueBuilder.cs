using System;
using System.Linq;
using System.Text.RegularExpressions;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public class EdFiDescriptorValueBuilder : IValueBuilder
    {
        private ILog _logger = LogManager.GetLogger(typeof(EdFiDescriptorValueBuilder));

        private readonly IApiSdkReflectionProvider apiSdkReflectionProvider;
        private readonly IItemFromApiProvider itemFromApiProvider;

        public EdFiDescriptorValueBuilder(IApiSdkReflectionProvider apiSdkReflectionProvider, IItemFromApiProvider itemFromApiProvider)
        {
            this.apiSdkReflectionProvider = apiSdkReflectionProvider;
            this.itemFromApiProvider = itemFromApiProvider;
        }

        private static Regex _termSplitter = new Regex("([A-Z]([A-Z]*)(?=[A-Z])|[A-Z][a-z0-9]+)", RegexOptions.Compiled);

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType != typeof(string))
                return ValueBuildResult.NotHandled;

            var lastSegment = buildContext.LogicalPropertyPath.Substring(buildContext.LogicalPropertyPath.LastIndexOf('.') + 1);

            if (!lastSegment.EndsWith("Descriptor", StringComparison.InvariantCultureIgnoreCase))
                return ValueBuildResult.NotHandled;

            var result = TryBuildValue(buildContext, lastSegment);

            // As a last ditch effort, try again, adding the containing type name as a prefix to the value (a code-gen'd naming convention)
            if (!result.Handled && buildContext.ContainingType != null)
            {
                string conventionBasedPropertyName = buildContext.ContainingType.Name + lastSegment;
                result = TryBuildValue(buildContext, conventionBasedPropertyName);
            }

            return result;
        }

        private ValueBuildResult TryBuildValue(BuildContext buildContext, string lastSegment)
        {
            //The properties often have names like "objectiveGradeLevelDescriptor".  So go in a loop, and if you can't find it, trim the first letter off and try again.
            string modelTypeName = lastSegment;

            string[] parts = null;
            int partIndex = 0;

            while (true)
            {
                Type modelType;

                _logger.DebugFormat("Trying to find descriptor model type '{0}'.", modelTypeName);

                if (apiSdkReflectionProvider.TryGetModelType(modelTypeName, out modelType))
                {
                    //Since descriptors don't implement any kind of interface, use dynamic to aquire the "codeValue" property.
                    dynamic descriptor = itemFromApiProvider.GetNext(modelType);

                    // Handle conditions with no data
                    if (descriptor == null)
                        return ValueBuildResult.WithValue(null, buildContext.LogicalPropertyPath);

                    string codeValue = descriptor.codeValue;
                    return ValueBuildResult.WithValue(codeValue, buildContext.LogicalPropertyPath);
                }

                // On first failure to locate due to application of role name, split the term into parts
                if (parts == null)
                {
                    var matches = _termSplitter.Matches(modelTypeName);
                    parts = matches.Cast<Match>().Select(m => m.Value).ToArray();
                }

                if (partIndex >= parts.Length)
                    break;

                // Trim the first word off the model name, and try again
                modelTypeName = string.Join(string.Empty, parts.Skip(partIndex++));

                if (partIndex == parts.Length && modelTypeName.Equals("Type"))
                    break;
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset() { }

        public ITestObjectFactory Factory { get; set; }
    }
}