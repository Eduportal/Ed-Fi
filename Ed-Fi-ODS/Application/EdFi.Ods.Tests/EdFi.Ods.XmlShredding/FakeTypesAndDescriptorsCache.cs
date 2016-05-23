namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding
{
    using System;
    using System.Collections.Generic;

    using global::EdFi.Ods.Entities.NHibernate;

    public class FakeTypesAndDescriptorsCache : TypesAndDescriptorsCache
    {
        public FakeTypesAndDescriptorsCache(Dictionary<Tuple<string, string>, int> idsByValue, Dictionary<Tuple<string, int>, string> valuesById) :base(idsByValue, valuesById)
        {
        }
    }
}