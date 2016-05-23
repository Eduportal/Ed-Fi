﻿using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
﻿using EdFi.Ods.Api.Models.Resources.CalendarDate;
﻿using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Post
{
    [TestFixture]
    class When_posting_a_resource_with_decimal_overflow : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_When_posting_a_resource_with_decimal_overflow_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }
        
        [Test]
        public void api_should_fail_with_500_code()
        {
            var calendarDate = new CalendarDate
            {
                Date = new DateTime(2015, 2, 17),
                CalendarDateCalendarEvents = new[]
                {
                    new CalendarDateCalendarEvent
                    {
                        CalendarEventDescriptor = "Instructional day",
                        EventDuration = 1123.2345498765M
                    }
                }
            };
            ((ICalendarDate) calendarDate).SchoolId = 255901001;

            string calendarDateJson = JsonConvert.SerializeObject(calendarDate);

            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "calendarDates"), new StringContent(calendarDateJson, Encoding.UTF8, "application/json")).Result;
                        response.IsSuccessStatusCode.ShouldBeFalse();
                        response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);

                        var result = response.Content.ReadAsStringAsync().Result;
                        var resource = JsonConvert.DeserializeObject<HttpError>(result);
                        resource["Message"].ToString().ShouldContain("The field EventDuration must be between");
                    }
                }
            }
        }
    }
}