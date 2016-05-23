using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Utilities.LoadGeneration.Infrastructure;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public abstract class RandomEducationOrganizationIdentifierValueBuilderBase : IValueBuilder
    {
        protected IEducationOrganizationIdentifiersProvider _educationOrganizationIdentifiersProvider;
        protected IRandom _random;
        protected List<int> _ids;

        protected abstract string HandledPropertyNameSuffix { get; }
        protected abstract IEnumerable<int> GetIdentifiers();

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (!buildContext.LogicalPropertyPath.EndsWith(HandledPropertyNameSuffix, StringComparison.InvariantCultureIgnoreCase)
                || buildContext.TargetType != typeof(int))
                return ValueBuildResult.NotHandled;

            int index = _random.Next(0, Ids.Count());

            return ValueBuildResult.WithValue(Ids[index], buildContext.LogicalPropertyPath);
        }

        public void Reset() { }
        public ITestObjectFactory Factory { get; set; }

        private List<int> Ids
        {
            get
            {
                if (_ids == null)
                    _ids = GetIdentifiers().ToList();

                return _ids;
            }
        }
    }
}