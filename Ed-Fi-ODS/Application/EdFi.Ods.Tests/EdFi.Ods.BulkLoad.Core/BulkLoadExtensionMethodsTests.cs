using EdFi.Ods.Tests._Extensions;

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core
{
    using System;
    using System.Collections.Generic;

    using global::EdFi.Ods.BulkLoad.Core;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_getting_loader_not_contained_in_dictionary
    {
        [Test]
        public void Should_throw_key_not_found_exception()
        {
            var dict = new Dictionary<Type, ILoadAggregates>();
            try
            {
                dict.GetAndRemoveLoaderFor(typeof(int));
            }
            catch (Exception exception)
            {
                exception.ShouldBeExceptionType<KeyNotFoundException>();
                exception.Message.ShouldEqual("Loader dictionary does not contain an entry for the type 'Int32'. Please verify that an aggregate loader (ILoadAggregates) exists for this type.");
            }          
        }
    }
}
