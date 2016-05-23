using System.Net;
using EdFi.Ods.Api.Pipelines.Student;
using EdFi.Ods.WebService.Tests._TestBases;
using EdFi.Ods.WebTest.Common.Extensions;
using NUnit.Framework;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Get
{
    [TestFixture]
    public class GetSudentsById_401_Tests : HttpTestBase
    {
        [Test]
        public void When_not_authenticated_Should_get_401_unauthorized()
        {
            var uri = BuildResourceUri(typeof(Student));
            var httpClientUnath = InitUnauthorizedClient();
            httpClientUnath.GetAsync(uri)
                           .WaitForResponse(HttpStatusCode.Unauthorized);
        }

        [Test]
        public void When_not_authenticated_and_requesting_invalid_resource_Should_get_401_unauthorized()
        {
            var uri = BuildResourceUri(typeof(Student));
            var httpClientUnauth = InitUnauthorizedClient();
            uri += "120c3jamc";
            httpClientUnauth.GetAsync(uri)
                            .WaitForResponse(HttpStatusCode.Unauthorized);
        }
    }
}
