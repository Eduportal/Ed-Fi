using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public abstract class When_building_references<TReference, TContainer> : TestFixtureBase where TContainer : new()
    {
        protected BuildContext CreateBuildContext(string suppliedLogicalPropertyPath,
            params object[] objectGraphAncestors)
        {
            var containingInstance = CreateContainingInstance();

            var graphLineage = new LinkedList<object>(objectGraphAncestors);
            graphLineage.AddFirst(containingInstance);

            return new BuildContext(
                suppliedLogicalPropertyPath,
                typeof (TReference),
                new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase),
                typeof (TContainer),
                graphLineage,
                BuildMode.Create);
        }

        protected virtual TContainer CreateContainingInstance()
        {
            return new TContainer();
        }
    }
}