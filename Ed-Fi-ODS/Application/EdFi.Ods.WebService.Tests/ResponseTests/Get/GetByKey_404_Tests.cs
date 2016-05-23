//using System.Net;

//using NUnit.Framework;

//namespace EdFi.Ods.WebService.Tests.ResponseTests.Get
//{
//    using EdFi.Ods.WebService.Tests.Extensions;

//    [TestFixture]
//    [Ignore("This should work with security - if key contains LEAID then security can short circuit otherwise should attempt retrieval and result in 404")]
//    //TODO: unignore and fix any contention with security
//    public class GetByKey_404_Tests : StudentResponseConvienenceBase
//    {
//        [Test]
//        public void When_Given_Valid_Argument_Of_0_But_No_Instance_Matches_Should_Return_404()
//        {
//            var key = "000000";
//            var client = GetClientWithPopulatedDatabase();
//            var uri = TestUrlBuilder.BuildResourceUri(EmptyResourceType,string.Format("{0}={1}", EmptyResourceKeyName, key));
//            client.GetAsync(uri).WaitForResponse(HttpStatusCode.NotFound);
//        }

//        [Test]
//        public void When_Given_Valid_Argument_But_No_Instance_Matches_Should_Return_404()
//        {
//            var key = "1";
//            var client = GetClientWithPopulatedDatabase();
//            var uri = TestUrlBuilder.BuildResourceUri(EmptyResourceType, string.Format("{0}={1}", EmptyResourceKeyName, key));
//            client.GetAsync(uri).WaitForResponse(HttpStatusCode.NotFound);
//        }
//    }
//}