using System;
using EdFi.Ods.Api.Models.Resources.Parent;
using EdFi.Ods.Api.Models.Resources.Student;
using EdFi.Ods.Api.Models.Resources.StudentParentAssociation;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace EdFi.Ods.WebService.Tests._Helpers
{
    internal class PersonHelperResponse
    {
        internal HttpResponseMessage ResponseMessage { get; set; }
        internal string UniqueId { get; set; }
    }

    internal class PersonHelper
    {
        private static string CreateResource(string resourceType, string uniqueId, string lastName, string firstName)
        {
            switch (resourceType)
            {
                case "students":
                    return ResourceHelper.CreateStudent(uniqueId, lastName, firstName);
                case "parents":
                    return ResourceHelper.CreateParent(uniqueId, lastName, firstName);
                case "staffs":
                    return ResourceHelper.CreateStaff(uniqueId, lastName, firstName);
            }

            return null;
        }

        internal static PersonHelperResponse CreatePerson(HttpClient client, string resource, string lastName, string firstName)
        {
            //var response = client.PostAsync(OwinUriHelper.BuildApiUri(null, "identities"), new StringContent(JsonConvert.SerializeObject(UniqueIdCreator.InitializeAPersonWithUniqueData()), Encoding.UTF8, "application/json")).Result;

            //if (!response.IsSuccessStatusCode)
            //    return new PersonHelperResponse { ResponseMessage = null, UniqueId = string.Empty };

            //var uniqueId = UniqueIdCreator.ExtractIdFromHttpResponse(response);
            var uniqueId = Guid.NewGuid().ToString("N");

            var createResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", resource), new StringContent(CreateResource(resource, uniqueId, lastName, firstName), Encoding.UTF8, "application/json")).Result;

            return new PersonHelperResponse { ResponseMessage = createResponse, UniqueId = uniqueId };
        }
    }

    internal class StudentHelper
    {
        internal static PersonHelperResponse CreateStudent(HttpClient client, string lastName, string firstName)
        {
            return PersonHelper.CreatePerson(client, "students", lastName, firstName);
        }

        internal static PersonHelperResponse CreateStudentAndAssociateToSchool(HttpClient client, string lastName, string firstName, int schoolId)
        {
            var studentResponse = CreateStudent(client, lastName, firstName);

            if (studentResponse.ResponseMessage.IsSuccessStatusCode)
            {
                var associationResult = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StudentSchoolAssociations"), 
                    new StringContent(ResourceHelper.CreateStudentSchoolAssociation(studentResponse.UniqueId, schoolId), Encoding.UTF8, "application/json")).Result;
            }

            return studentResponse;
        }

        internal static HttpResponseMessage CreateStudentParentAssociation(HttpClient client, string studentUniqueId, string parentUniqueId)
        {
            var association = new StudentParentAssociation
            {
                StudentReference = new StudentReference { StudentUniqueId = studentUniqueId },
                ParentReference = new ParentReference { ParentUniqueId = parentUniqueId },
            };

            return client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StudentParentAssociations"), new StringContent(JsonConvert.SerializeObject(association), Encoding.UTF8, "application/json")).Result;
        }
    }

    internal class ParentHelper
    {
        internal static PersonHelperResponse CreateParent(HttpClient client, string lastName, string firstName)
        {
            return PersonHelper.CreatePerson(client, "parents", lastName, firstName);
        }
    }

    internal class StaffHelper
    {
        internal static PersonHelperResponse CreateStaff(HttpClient client, string lastName, string firstName)
        {
            return PersonHelper.CreatePerson(client, "staffs", lastName, firstName);
        }
    }
}
