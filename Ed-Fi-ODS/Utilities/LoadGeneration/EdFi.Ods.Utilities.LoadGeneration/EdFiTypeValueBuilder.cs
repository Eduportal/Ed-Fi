using System;
using System.Collections.Generic;
using EdFi.Ods.Tests.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class EdFiTypeValueBuilder : IValueBuilder
    {
        private readonly IApiSdkReflectionProvider apiSdkReflectionProvider;
        private readonly IItemFromApiProvider itemFromApiProvider;

        public EdFiTypeValueBuilder(IApiSdkReflectionProvider apiSdkReflectionProvider, IItemFromApiProvider itemFromApiProvider)
        {
            this.apiSdkReflectionProvider = apiSdkReflectionProvider;
            this.itemFromApiProvider = itemFromApiProvider;
        }

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType != typeof(string))
                return ValueBuildResult.NotHandled;

            var currentSegment = buildContext.LogicalPropertyPath.Substring(buildContext.LogicalPropertyPath.LastIndexOf('.') + 1);

            if(!currentSegment.EndsWith("Type"))
                return ValueBuildResult.NotHandled;

            //The properties often have names like "somethingSexType".  So go in a loop, and if you can't find it, trim the first letter off and try again.
            var modelTypeName = currentSegment;
            while (modelTypeName.Length > 0)
            {
                var modelType = apiSdkReflectionProvider.GetModelType(modelTypeName);
                if (modelType != null)
                {
                    //Since Types don't implement any kind of interface, use dynamic to aquire the "shortDescription" property.
                    dynamic descriptor = itemFromApiProvider.CreateNext(modelType);
                    string shortDescription = descriptor.shortDescription;

                    var buildResult = ValueBuildResult.WithValue(shortDescription, buildContext.LogicalPropertyPath);
                    return buildResult;
                }

                modelTypeName = modelTypeName.Substring(1);
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset()
        {
        }

        public ITestObjectFactory Factory { get; set; }
    }
}