using System;
using System.Collections.Generic;
using EdFi.Ods.Admin.Models;
using System.Linq;

namespace EdFi.Ods.Admin.DeployedUsers
{
    /// <summary>
    /// This class holds default users that can be deployed with an Instance of the API
    ///
    /// BuiltinUsers: These types of users are intended to be deployed on any given instance and should be safe for use
    /// 
    /// TestUsers: These types of users should only be deployed on an instance for testing purposes and are not safe for production use
    /// </summary>
    public static class DeployedAdminUsers
    {
        private static readonly IDeployedUser BuiltinAdmin = new BuiltinUser
                                                    {
                                                        Name = "Ed-Fi Alliance, Admin User",
                                                        Email = "test@edfi.org",
                                                        Password = "***REMOVED***",
                                                        Roles = new[] { "Administrator" }
                                                    };

        private static readonly IDeployedUser JoeAdmin
            = new TestUser
                  {
                      Name = "JoeAdmin",
                      Email = "joeismad2093@gmail.com",
                      Roles = new[] { "Administrator" },
                      ClientApis =
                          new[]
                              {
                                  new TestClientApi
                                      {
                                          Key = "JoeAdmin_Minimal_Key",
                                          Secret = "JoeAdmin_Minimal_Secret",
                                          Name = "JoesMinimalApplicaiton",
                                          SandboxType = SandboxType.Minimal,
                                          FakeSandbox = true,
                                      }
                              }
                  };

        private static readonly IDeployedUser SaraUser
            = new TestUser
                  {
                      Name = "SaraUser",
                      Email = "sarauserdlptest@gmail.com",
                      ClientApis =
                          new[]
                              {
                                  new TestClientApi
                                      {
                                          Key = "SaraUser_Populated_Key",
                                          Secret = "SaraUser_Populated_Secret",
                                          Name = "SarasPopulatedApplicaiton",
                                          SandboxType = SandboxType.Sample,
                                          FakeSandbox = true,
                                      },

                                  new TestClientApi
                                      {
                                          Key = "SaraUser_Minimal_Key",
                                          Secret = "SaraUser_Minimal_Secret",
                                          Name = "SarasMinimalApplicaiton",
                                          SandboxType = SandboxType.Minimal,
                                          FakeSandbox = true,
                                      }
                              }
                  };
        
        private static readonly IDeployedUser XmlTest = new TestUser
        {
            Name = "XmlTest",
            Email = "xmltest@doublelinepartners.com",//This email address does not exist and should not be used for email
            ClientApis = new[]
            {
                new TestClientApi
                {
                    Key = "XmlTest_Minimal_Key",
                    Secret = "XmlTest_Minimal_Secret",
                    Name = "XmlTest Application",
                    SandboxType = SandboxType.Minimal,
                    FakeSandbox = false,
                }
            }
        };

        //NOTE:  If you are adding or removing users, make sure to also check the configured test users in the "TestUsers" setting
        // 
        //       We have this information in both places to double-check the test users that are getting deployed, and because
        //       we don't currently deploy all test users to the build environment.
        private static readonly IDeployedUser[] DeployableUsers = { BuiltinAdmin, JoeAdmin, SaraUser, XmlTest };

        public static IDeployedUser[] GetTestUsers(IEnumerable<string> names)
        {
            var users = new HashSet<IDeployedUser>();
            foreach (var name in names)
            {
                var testUser = GetNamedUser(name);
                users.Add(testUser);
            }
            return users.ToArray();
        }

        public static IDeployedUser GetNamedUser(string name)
        {
            var testUser = DeployableUsers.SingleOrDefault(x => x.Name.ToLowerInvariant().Equals(name.ToLowerInvariant()));
            if (testUser == null)
                throw new Exception(string.Format("Could not locate test user with name '{0}'", name));
            return testUser;
        }
    }
}