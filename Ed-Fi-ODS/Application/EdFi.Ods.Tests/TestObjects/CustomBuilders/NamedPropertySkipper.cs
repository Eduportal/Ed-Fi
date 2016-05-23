using EdFi.TestObjects;

namespace EdFi.Ods.Tests.TestObjects.CustomBuilders
{
    using System;
    using System.Linq;

    public class NamedPropertySkipper<T> : IValueBuilder
    {
        private readonly string _propertyName;

        public NamedPropertySkipper(string propertyName)
        {
            this._propertyName = propertyName;
        }

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            string logicalPropertyPath = buildContext.LogicalPropertyPath;

            var propertyName = logicalPropertyPath.Split('.').Last();
            if (propertyName.Equals(this._propertyName) && buildContext.TargetType == typeof (T))
            {
                return ValueBuildResult.Skip(logicalPropertyPath);
            }
            return ValueBuildResult.NotHandled;
        }

        public void Reset()
        {
        }

        public ITestObjectFactory Factory { get; set; }
    }
}