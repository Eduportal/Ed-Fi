using System;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    public class EntityParentBackreferenceValueBuilder : IValueBuilder
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(EntityParentBackreferenceValueBuilder));

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            Type parentType = buildContext.GetParentType();

            if (parentType != null)
            {
                if (parentType == buildContext.TargetType || buildContext.TargetType.IsAssignableFrom(parentType))
                    return ValueBuildResult.WithValue(
                        buildContext.GetParentInstance(), 
                        buildContext.LogicalPropertyPath);
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset() { }

        public ITestObjectFactory Factory { get; set; }
    }
}