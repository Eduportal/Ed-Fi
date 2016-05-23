using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EdFi.Common.Database;
using EdFi.Ods.Security.Authorization;
using EdFi.Security;
using EdFi.Security.Authorization;
using NUnit.Framework;
using Should;
using UnitTests.EdFi.Ods.Security.Utilities.CastleWindsor;
using List = NHibernate.Mapping.List;

namespace UnitTests.EdFi.Security.Authorization
{
    [TestFixture]
    public class When_executing_with_null_authorization_rules
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Should_return_argument_null_exception()
        {
            var executor = new EdFiOdsAuthorizationRulesExecutor(new NullDatabaseConnectionStringProvider());
            executor.Execute(null);
        }
    }

    [TestFixture]
    public class When_executing_with_no_authorization_rules
    {
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_return_argument_exception()
        {
            var executor = new EdFiOdsAuthorizationRulesExecutor(new NullDatabaseConnectionStringProvider());
            executor.Execute(new List<AuthorizationRule>());
        }
    }

    [TestFixture]
    public class When_executing_with_authorization_rules_not_having_choice_values
    {
        [Test]
        [ExpectedException(typeof(EdFiSecurityException))]
        public void Should_return_argument_exception()
        {
            var executor = new EdFiOdsAuthorizationRulesExecutor(new NullDatabaseConnectionStringProvider());
            executor.Execute(new List<AuthorizationRule>{AuthorizationRule.Create(new Tuple<string,string>("a", "b"), new List<Tuple<string,string>>())});
        }
    }
}
