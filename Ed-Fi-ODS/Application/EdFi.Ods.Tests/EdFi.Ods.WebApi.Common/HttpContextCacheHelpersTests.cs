namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Common
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    using global::EdFi.Ods.Api.Common.Authorization;

    using NUnit.Framework;

    using Should;

    using Subtext.TestLibrary;

    [TestFixture]
    public class HttpContextCacheHelpersTests
    {
        [Test]
        public void Should_Cache_item_in_HttpContext_Items()
        {
            using (var simulatedContext = new HttpSimulator())
            {
                var key = "abc";
                var expected = "XYZ";
                simulatedContext.SimulateRequest(new Uri("http://abc.com"));
                HttpContext.Current.CacheInRequest(key, expected);
                var actual = HttpContext.Current.Items[key];
                actual.ShouldEqual(expected);
            }
        }

        [Test]
        public void Should_Not_Try_To_Add_Item_If_Already_In_Cache()
        {
            using (var simContext = new HttpSimulator())
            {
                var key = "abc";
                var expected = "XYZ";
                simContext.SimulateRequest(new Uri("http://yes.com"));
                HttpContext.Current.CacheInRequest(key, expected);
                HttpContext.Current.CacheInRequest(key, expected);
                var actual = HttpContext.Current.Items[key];
                actual.ShouldEqual(expected);
            }
        }

        // TODO: GKM - review these tests

        //[Test]
        //public void Should_Return_True_If_Item_is_In_Cache()
        //{
        //    using (var simulatedContext = new HttpSimulator())
        //    {
        //        var key = "abc";
        //        var expected = "XYZ";
        //        simulatedContext.SimulateRequest(new Uri("http://abc.com"));
        //        HttpContext.Current.CacheInRequest(key, expected);
        //        HttpContext.Current.RequestCacheContains(key, expected).ShouldBeTrue();
        //    }
            
        //}

        [Test]
        public void Should_Retrieve_Item_from_Cache()
        {
            using (var simulatedContext = new HttpSimulator())
            {
                var key = "abc";
                var expected = "XYZ";
                simulatedContext.SimulateRequest(new Uri("http://abc.com"));
                HttpContext.Current.CacheInRequest(key, expected);
                var actual = HttpContext.Current.RetrieveFromRequestCache<string>(key);
                actual.ShouldEqual(expected);
            }
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException), ExpectedMessage = "was not a key", MatchType = MessageMatch.Contains)]
        public void Should_Throw____Exception_When_Key_Is_Not_In_Cache()
        {
            using (var simulatedContext = new HttpSimulator())
            {
                simulatedContext.SimulateRequest(new Uri("http://abc.com"));
                HttpContext.Current.RetrieveFromRequestCache<string>("NOT IN CACHE").ShouldNotBeNull();
            }
        }
    }
}