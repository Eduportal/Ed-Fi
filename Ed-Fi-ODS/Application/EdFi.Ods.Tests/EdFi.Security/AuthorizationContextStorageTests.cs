using EdFi.Ods.Tests._Extensions;

namespace EdFi.Ods.Tests.EdFi.Security
{
    using System;

    using global::EdFi.Common.Context;
    using global::EdFi.Common.Security.Claims;

    using NUnit.Framework;

    using Should;

    public class AuthorizationContextStorageTests
    {
        [TestFixture]
        public class When_calling_the_VerifyContextSet_method_with_no_action_stored_in_the_current_context
        {
            [Test]
            public void Should_throw_an_AuthorizationContextException_with_action_in_the_message()
            {
                // Set the "resource" context only
                var contextStorage = new HashtableContextStorage();
                contextStorage.SetValue(AuthorizationContextKeys.Resource, "Some Resource");
                var sut = new AuthorizationContextProvider(contextStorage);

                Exception actualException = null;
                try
                {
                    sut.VerifyAuthorizationContextExists();
                }
                catch (Exception ex)
                {
                    actualException = ex;
                }

                actualException.ShouldBeExceptionType<AuthorizationContextException>();
                actualException.Message.ShouldContain("action");
            }
        }


        [TestFixture]
        public class When_calling_the_VerifyContextSet_method_with_no_resource_stored_in_the_current_context
        {
            [Test]
            public void Should_throw_an_AuthorizationContextException_with_action_in_the_message()
            {
                // Set the "resource" context only
                var contextStorage = new HashtableContextStorage();
                contextStorage.SetValue(AuthorizationContextKeys.Action, "Some Action");
                var sut = new AuthorizationContextProvider(contextStorage);

                Exception actualException = null;
                try
                {
                    sut.VerifyAuthorizationContextExists();
                }
                catch (Exception ex)
                {
                    actualException = ex;
                }

                actualException.ShouldBeExceptionType<AuthorizationContextException>();
                actualException.Message.ShouldContain("resource");
            }
        }

        [TestFixture]
        public class When_calling_the_VerifyContextSet_method_with_a_valid_context
        {
            [Test]
            public void Should_not_throw_an_AuthorizationContextException()
            {
                // Set the "resource" context only
                var contextStorage = new HashtableContextStorage();
                contextStorage.SetValue(AuthorizationContextKeys.Resource, "Some Resource");
                contextStorage.SetValue(AuthorizationContextKeys.Action, "Some Action");
                var sut = new AuthorizationContextProvider(contextStorage);

                sut.VerifyAuthorizationContextExists();
            }
        }
    }
}
