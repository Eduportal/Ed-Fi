//using System.Net;

//using NUnit.Framework;

//namespace EdFi.Ods.WebService.Tests.ResponseTests.Get
//{
//    using EdFi.Ods.WebService.Tests.Extensions;

//    [TestFixture]
//    public class GetByKey_400_Tests : StudentResponseConvienenceBase
//    {
//        //This is a known bug and cannot be fixed without moving to Web Api 2.0
//        [Test]
//        [Ignore("This test is obsolete after auth is implemented. Cannot get by key for an unrelated resource.")]
//        public void When_Given_An_Invalid_Value_for_Key_Should_Return_400()
//        {
//            var key = "@#$";
//            var uri = TestUrlBuilder.BuildResourceUri(GoodResourceType, string.Format("{0}={1}", GoodResourceKeyName, key));
//            var client = GetClientWithPopulatedDatabase();
//            client.GetAsync(uri).WaitForResponse(HttpStatusCode.BadRequest);
//        }
//    }
//}