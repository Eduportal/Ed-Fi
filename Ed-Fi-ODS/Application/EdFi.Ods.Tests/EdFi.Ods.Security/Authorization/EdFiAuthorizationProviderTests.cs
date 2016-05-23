using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using EdFi.Common.Context;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Common.Security;
using EdFi.Ods.Security.Authorization;
using EdFi.Ods.Security.AuthorizationStrategies;
using EdFi.Ods.Security.AuthorizationStrategies.NoFurtherAuthorization;
using EdFi.Ods.Security.AuthorizationStrategies.Relationships;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.Tests.EdFi.Ods.Security._Stubs;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using QuickGraph;
using Rhino.Mocks;
using Should;
using Action = EdFi.Ods.Security.Metadata.Models.Action;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.Authorization
{
    // --------------------------------------------------------------------------------------------
    // NOTE: This unit test style is exploratory, looking for ways to make the unit tests easier to
    // understand from a code review and maintenance perspective.
    // --------------------------------------------------------------------------------------------
    public class Feature_Validating_the_incoming_AuthorizationContext
    {
        public class When_attempting_to_authorize_with_a_null_authorization_context : TestFixtureBase
        {
            protected override void Act()
            {
                // Create provider with stubs
                var provider = new EdFiAuthorizationProvider(
                    Stub<IResourceAuthorizationMetadataProvider>(),
                    new IEdFiAuthorizationStrategy[0],
                    Stub<ISecurityRepository>());

                provider.AuthorizeSingleItem(null);
            }

            [Assert]
            public void Should_throw_an_ArgumentNullException()
            {
                ActualException.ShouldBeExceptionType<ArgumentNullException>();
            }
        }

        public class When_attempting_to_authorize_without_a_resource_value : TestFixtureBase
        {
            protected override void Act()
            {
                // Execute code under test
                var provider = new EdFiAuthorizationProvider(
                    Stub<IResourceAuthorizationMetadataProvider>(),
                    new IEdFiAuthorizationStrategy[0], 
                    Stub<ISecurityRepository>());

                provider.AuthorizeSingleItem(new EdFiAuthorizationContext(new ClaimsPrincipal(), " ", "action", new object()));
            }

            [Assert]
            public void Should_throw_an_AuthorizationContextException()
            {
                ActualException.ShouldBeExceptionType<AuthorizationContextException>();
                ActualException.Message.ShouldContain("resource");
            }
        }

        public class When_attempting_to_authorize_with_a_null_resource : TestFixtureBase
        {
            protected override void Act()
            {
                // Execute code under test
                var provider = new EdFiAuthorizationProvider(
                    Stub<IResourceAuthorizationMetadataProvider>(),
                    new IEdFiAuthorizationStrategy[0], 
                    Stub<ISecurityRepository>());

                provider.AuthorizeSingleItem(new EdFiAuthorizationContext(new ClaimsPrincipal(), null, "action", new object()));
            }

            [Assert]
            public void Should_throw_an_AuthorizationContextException()
            {
                ActualException.ShouldBeExceptionType<ArgumentNullException>();
                ((ArgumentNullException)ActualException).ParamName.ShouldEqual("resource");
            }
        }

        public class When_attempting_to_authorize_without_an_action_value : TestFixtureBase
        {
            protected override void Act()
            {
                // Execute code under test
                var provider = new EdFiAuthorizationProvider(
                    Stub<IResourceAuthorizationMetadataProvider>(),
                    new IEdFiAuthorizationStrategy[0],
                    Stub<ISecurityRepository>());

                provider.AuthorizeSingleItem(new EdFiAuthorizationContext(new ClaimsPrincipal(), "resource", " ", new object()));
            }

            [Assert]
            public void Should_throw_an_AuthorizationContextException()
            {
                ActualException.ShouldBeType<AuthorizationContextException>();
                ActualException.Message.ShouldContain("action");
            }
        }

        public class When_attempting_to_authorize_with_a_null_action : TestFixtureBase
        {
            protected override void Act()
            {
                // Execute code under test
                var provider = new EdFiAuthorizationProvider(
                    Stub<IResourceAuthorizationMetadataProvider>(),
                    new IEdFiAuthorizationStrategy[0],
                    Stub<ISecurityRepository>());

                provider.AuthorizeSingleItem(new EdFiAuthorizationContext(new ClaimsPrincipal(), "resource", null, new object()));
            }

            [Assert]
            public void Should_throw_an_AuthorizationContextException()
            {
                ActualException.ShouldBeExceptionType<AuthorizationContextException>();
                ActualException.Message.ShouldContain("action");
            }
        }

        public class When_attempting_to_authorize_with_multiple_resources : TestFixtureBase
        {
            protected override void Act()
            {
                // Execute code under test
                var provider = new EdFiAuthorizationProvider(
                    Stub<IResourceAuthorizationMetadataProvider>(),
                    new IEdFiAuthorizationStrategy[0],
                    Stub<ISecurityRepository>());

                provider.AuthorizeSingleItem(
                    new EdFiAuthorizationContext(
                        new ClaimsPrincipal(),
                        new Collection<Claim> { new Claim("type", "value"), new Claim("type", "value") }, 
                        new Collection<Claim> { new Claim("action", "value") }, 
                        new object()));
            }

            [Assert]
            public void Should_throw_an_AuthorizationContextException()
            {
                ActualException.ShouldBeType<AuthorizationContextException>();
                ActualException.Message.ShouldContain("single resource");
            }
        }

        public class When_attempting_to_authorize_with_multiple_actions : TestFixtureBase
        {
            protected override void Act()
            {
                // Execute code under test
                var provider = new EdFiAuthorizationProvider(
                    Stub<IResourceAuthorizationMetadataProvider>(),
                    new IEdFiAuthorizationStrategy[0],
                    Stub<ISecurityRepository>());

                provider.AuthorizeSingleItem(
                    new EdFiAuthorizationContext(
                        new ClaimsPrincipal(),
                        new Collection<Claim> { new Claim("resource", "value") },
                        new Collection<Claim> { new Claim("type", "value"), new Claim("type", "value") },
                        new object()));
            }

            [Assert]
            public void Should_throw_an_AuthorizationContextException()
            {
                ActualException.ShouldBeExceptionType<AuthorizationContextException>();
                ActualException.Message.ShouldContain("single action");
            }
        }    
    }

    public class Feature_Validating_authorization_strategy_naming_conventions
    {
        private class AuthorizationStrategyNotFollowingConventions : IEdFiAuthorizationStrategy
        {
            public void AuthorizeSingleItem(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext)
            {
                throw new NotImplementedException();
            }

            public void ApplyAuthorizationFilters(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext, ParameterizedFilterBuilder filterBuilder)
            {
                throw new NotImplementedException();
            }
        }

        private class ConventionFollowingAuthorizationStrategy : IEdFiAuthorizationStrategy
        {
            public void AuthorizeSingleItem(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext)
            {
                throw new NotImplementedException();
            }

            public void ApplyAuthorizationFilters(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext, ParameterizedFilterBuilder filterBuilder)
            {
                throw new NotImplementedException();
            }
        }

        private class ConventionFollowing2AuthorizationStrategy 
            : ConventionFollowingAuthorizationStrategy { }

        public class When_creating_the_authorization_provider_with_all_authorization_strategy_types_whose_names_end_with_AuthorizationStrategy 
            : TestFixtureBase
        {
            protected override void Act()
            {
                // Execute code under test
                var authorizationStrategies = new IEdFiAuthorizationStrategy[]
                {
                    new ConventionFollowingAuthorizationStrategy(),
                    new ConventionFollowing2AuthorizationStrategy(),
                };

                var provider = new EdFiAuthorizationProvider(
                    Stub<IResourceAuthorizationMetadataProvider>(),
                    authorizationStrategies,
                    Stub<ISecurityRepository>());
            }

            [Assert]
            public void Should_not_throw_an_exception()
            {
                ActualException.ShouldBeNull();
            }
        }

        public class When_creating_the_authorization_provider_with_an_authorization_strategy_type_whose_name_does_not_end_with_AuthorizationStrategy 
            : TestFixtureBase
        {
            protected override void Act()
            {
                // Execute code under test
                var authorizationStrategies = new IEdFiAuthorizationStrategy[]
                {
                    new ConventionFollowingAuthorizationStrategy(),
                    new AuthorizationStrategyNotFollowingConventions(),
                };

                var provider = new EdFiAuthorizationProvider(
                    Stub<IResourceAuthorizationMetadataProvider>(),
                    authorizationStrategies,
                    Stub<ISecurityRepository>());
            }

            [Assert]
            public void Should_throw_an_ArgumentException_indicating_that_the_authorization_strategy_doesnt_follow_proper_naming_conventions()
            {
                ActualException.ShouldBeExceptionType<ArgumentException>();
                ActualException.Message.ShouldContain(typeof(AuthorizationStrategyNotFollowingConventions).Name);
            }
        }
    }

    public class Feature_Authorization_strategy_selection
    {
        public abstract class AuthorizationStrategyBase : IEdFiAuthorizationStrategy
        {
            public bool SingleItemWasCalled { get; private set; }
            public bool FilteringWasCalled { get; private set; }

            public void AuthorizeSingleItem(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext)
            {
                SingleItemWasCalled = true;
            }

            public void ApplyAuthorizationFilters(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext, ParameterizedFilterBuilder filterBuilder)
            {
                FilteringWasCalled = true;
            }
        }

        public class FirstAuthorizationStrategy : AuthorizationStrategyBase { }
        public class SecondAuthorizationStrategy : AuthorizationStrategyBase { }
        public class FourthAuthorizationStrategy : AuthorizationStrategyBase { }

        public abstract class When_authorizing_a_request : TestFixtureBase
        {
            // Claims represent a lineage (1 is the leaf, 4 is the root)
            protected const string Resource1ClaimUri = @"http://CLAIMS/resource1";
            protected const string Resource2ClaimUri = @"http://CLAIMS/resource2";
            protected const string Resource3ClaimUri = @"http://CLAIMS/resource3";
            protected const string Resource4ClaimUri = @"http://CLAIMS/resource4";

            protected const string ReadActionUri = @"http://ACTIONS/read";

            protected readonly FirstAuthorizationStrategy FirstAuthorizationStrategy = new FirstAuthorizationStrategy();
            protected readonly SecondAuthorizationStrategy SecondAuthorizationStrategy = new SecondAuthorizationStrategy();
            protected readonly FourthAuthorizationStrategy FourthAuthorizationStrategy = new FourthAuthorizationStrategy();

            // Define all authorization strategies
            protected IEdFiAuthorizationStrategy[] AuthorizationStrategies;
            protected ISecurityRepository SecurityRepository;

            protected When_authorizing_a_request()
            {
                SecurityRepository = Given_a_security_repository_returning_all_actions();

                AuthorizationStrategies = new IEdFiAuthorizationStrategy[]
                {
                    FirstAuthorizationStrategy,
                    SecondAuthorizationStrategy,
                    FourthAuthorizationStrategy,
                };
            }

            protected ClaimsPrincipal Given_a_principle_with_a_single_resource_claim(string resourceClaimUri, string actionUri)
            {
                // Issue Resource claim with action
                Claim[] claims = 
                {
                    JsonClaimHelper.CreateClaim(resourceClaimUri, new EdFiResourceClaimValue(actionUri))
                };

                return
                    new ClaimsPrincipal(
                        new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));
            }

            private ISecurityRepository Given_a_security_repository_returning_all_actions()
            {
                var securityRepository = MockRepository.GenerateStub<ISecurityRepository>();
                securityRepository.Stub(sr => sr.GetActionByName("Create")).Return(new Action { ActionId = 1, ActionName = "Create", ActionUri = "http://ACTIONS/create" });
                securityRepository.Stub(sr => sr.GetActionByName("Read")).Return(new Action { ActionId = 1, ActionName = "Read", ActionUri = "http://ACTIONS/read" });
                securityRepository.Stub(sr => sr.GetActionByName("Update")).Return(new Action { ActionId = 1, ActionName = "Update", ActionUri = "http://ACTIONS/update" });
                securityRepository.Stub(sr => sr.GetActionByName("Delete")).Return(new Action { ActionId = 1, ActionName = "Delete", ActionUri = "http://ACTIONS/delete" });

                return securityRepository;
            }

            protected IResourceAuthorizationMetadataProvider CreateResourceAuthorizationMetadataProvider(string resourceClaim, string actionUri)
            {
                // Return metadata with 4 resource claims, with Resource 3 not having an authorization strategy.
                var authorizationMetadataProvider = Stub<IResourceAuthorizationMetadataProvider>();
                authorizationMetadataProvider.Stub(
                    x =>
                        x.GetResourceClaimAuthorizationStrategies(resourceClaim, actionUri))
                                             .Return(
                                                 (new List<ResourceClaimAuthorizationStrategy>
                                                 {
                                                     new ResourceClaimAuthorizationStrategy
                                                     {
                                                         ClaimName = Resource1ClaimUri,
                                                         AuthorizationStrategy = null, //"First"
                                                     },
                                                     new ResourceClaimAuthorizationStrategy
                                                     {
                                                         ClaimName = Resource2ClaimUri,
                                                         AuthorizationStrategy = "Second"
                                                     },
                                                     new ResourceClaimAuthorizationStrategy
                                                     {
                                                         ClaimName = Resource3ClaimUri,
                                                         AuthorizationStrategy = null
                                                     },
                                                     new ResourceClaimAuthorizationStrategy
                                                     {
                                                         ClaimName = Resource4ClaimUri,
                                                         AuthorizationStrategy = "Fourth"
                                                     },
                                                 })
                                                 // Trim out lineage from bottom up to incoming claim name
                                                 .SkipWhile(rcas => !rcas.ClaimName.Equals(resourceClaim, StringComparison.InvariantCultureIgnoreCase))
                                                 .ToList());

                return authorizationMetadataProvider;
            }
        }

        public class When_authorizing_a_request_for_which_the_principal_has_1_matching_claim_with_an_authorization_strategy_defined
            : When_authorizing_a_request
        {
            // Supplied values

            // Actual values

            // Dependencies

            protected override void Arrange()
            {
                // Initialize dependencies
            }

            protected override void Act()
            {
                // Caller has Read access to Resource 2
                var claimsPrincipal = Given_a_principle_with_a_single_resource_claim(Resource2ClaimUri, ReadActionUri);

                // Request is for Read access to Resource 1
                var authorizationContext = new EdFiAuthorizationContext(
                    claimsPrincipal,
                    Resource1ClaimUri,
                    ReadActionUri,
                    new object());

                // Get the strategy metadata provider, using the authorization context values
                var authorizationMetadataProvider = CreateResourceAuthorizationMetadataProvider(
                    authorizationContext.Resource.Single().Value,
                    authorizationContext.Action.Single().Value);

                var provider = new EdFiAuthorizationProvider(
                    authorizationMetadataProvider,
                    AuthorizationStrategies,
                    SecurityRepository);

                provider.AuthorizeSingleItem(authorizationContext);
            }

            [Assert]
            public void Should_attempt_to_authorize_using_the_lowest_level_resource_claim_with_an_authorization_strategy()
            {
                SecondAuthorizationStrategy.SingleItemWasCalled.ShouldBeTrue();
            }

            [Assert]
            public void Should_not_attempt_to_authorize_using_any_other_authorization_strategies()
            {
                FourthAuthorizationStrategy.SingleItemWasCalled.ShouldBeFalse();
            }
        }

        #region Temporarily deprecated unit test - should be re-evaluated after support for claimset specific authorization strategies overrides is added

        //public class When_authorizing_a_request_for_which_the_principal_has_1_matching_claim_without_an_authorization_strategy_defined
        //: When_authorizing_a_request
        //{
        //    // Supplied values

        //    // Actual values

        //    // Dependencies

        //    protected override void Arrange()
        //    {
        //        // Initialize dependencies
        //    }

        //    protected override void Act()
        //    {
        //        // Caller has Read access to Resource 3 (with no associated authorization strategy)
        //        var claimsPrincipal = Given_a_principle_with_a_single_resource_claim(Resource3ClaimUri, ReadActionUri);

        //        // Request is for Read access to Resource 2
        //        var authorizationContext = new EdFiAuthorizationContext(
        //            claimsPrincipal,
        //            Resource2ClaimUri,
        //            ReadActionUri,
        //            new object());

        //        // Get the strategy metadata provider, using the authorization context values
        //        var authorizationMetadataProvider = CreateResourceAuthorizationMetadataProvider(
        //            authorizationContext.Resource.Single().Value,
        //            authorizationContext.Action.Single().Value);

        //        var provider = new EdFiAuthorizationProvider(
        //            authorizationMetadataProvider,
        //            AuthorizationStrategies,
        //            SecurityRepository);

        //        provider.AuthorizeSingleItem(authorizationContext);
        //    }

        //    [Assert]
        //    public void Should_attempt_to_authorize_using_the_next_higher_up_resource_claim_with_an_assigned_authorization_strategy()
        //    {
        //        FourthAuthorizationStrategy.SingleItemWasCalled.ShouldBeTrue();
        //    }

        //    [Assert]
        //    public void Should_not_attempt_to_authorize_using_any_other_authorization_strategies()
        //    {
        //        FirstAuthorizationStrategy.SingleItemWasCalled.ShouldBeFalse();
        //        SecondAuthorizationStrategy.SingleItemWasCalled.ShouldBeFalse();
        //    }
        //}

        #endregion


        public class When_authorizing_a_request_for_which_the_principal_has_multiple_matching_claims
            : When_authorizing_a_request
        {
            protected override void Arrange()
            {
            }

            protected override void Act()
            {
                // Caller has claims for Resources 2 and 3 (out of order)
                Claim[] claims = 
                {
                    // The "out of order" of these claims is intentional and is testing that the match is 
                    // made on the first matching claim based on the authorization metadata hierarchy
                    // rather than the order in which the claims are issued to the caller in the claim set.
                    // In this case, Resource2 is "lower" in the hierarchy, and should be the one matched first.
                    JsonClaimHelper.CreateClaim(Resource3ClaimUri, new EdFiResourceClaimValue(ReadActionUri)),
                    JsonClaimHelper.CreateClaim(Resource2ClaimUri, new EdFiResourceClaimValue(ReadActionUri)),
                };

                var claimsPrincipal =
                    new ClaimsPrincipal(
                        new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));

                // Request is for Read access to Resource 1
                var authorizationContext = new EdFiAuthorizationContext(
                    claimsPrincipal,
                    Resource1ClaimUri,
                    ReadActionUri,
                    new object());

                // Get the strategy metadata provider, using the authorization context values
                var authorizationMetadataProvider = CreateResourceAuthorizationMetadataProvider(
                    authorizationContext.Resource.Single().Value,
                    authorizationContext.Action.Single().Value);

                var provider = new EdFiAuthorizationProvider(
                    authorizationMetadataProvider,
                    AuthorizationStrategies,
                    SecurityRepository);

                provider.AuthorizeSingleItem(authorizationContext);
            }

            [Assert]
            public void Should_resolve_claims_using_authorization_metadata_order_rather_than_callers_claims_order_and_invoke_lowest_matching_claims_authorization_strategy()
            {
                SecondAuthorizationStrategy.SingleItemWasCalled.ShouldBeTrue();
                FourthAuthorizationStrategy.SingleItemWasCalled.ShouldBeFalse();
            }
        }
    }

    public class Feature_Authorizing_requests_focusing_on_actions
    {
        public abstract class When_authorizing : TestFixtureBase
        {
            protected const string TestResource = "http://ed-fi.org/ods/resource/test";

            protected string SuppliedPrincipalClaim;
            protected string SuppliedPrincipalAction;
            protected string SuppliedRequestedAction;
            protected string SuppliedResourceAuthorizationClaim;
            protected string SuppliedResourceAuthorizationStrategy;
            private EdFiAuthorizationContext _suppliedEdFiAuthorizationContext;

            private IResourceAuthorizationMetadataProvider _resourceAuthorizationMetadataProvider;
            private IEducationOrganizationHierarchyProvider _educationOrganizationHierarchyProvider;
            private StubSecurityRepository _securityRepository;

            protected override void Arrange()
            {
                SetPrincipalAndStrategyAndContextValues();

                var suppliedResourceClaimsAuthorizationStrategyMetadata =
                    GetEdFiClaimsAuthorizationStrategyMetadata(SuppliedResourceAuthorizationStrategy, SuppliedResourceAuthorizationClaim);

                _suppliedEdFiAuthorizationContext = GetEdFiAuthorizationContext(SuppliedRequestedAction);

                _resourceAuthorizationMetadataProvider = Stub<IResourceAuthorizationMetadataProvider>();
                _resourceAuthorizationMetadataProvider.Stub(x => x.GetResourceClaimAuthorizationStrategies(null, null))
                    .IgnoreArguments()
                    .Return(suppliedResourceClaimsAuthorizationStrategyMetadata);

                var edOrgCache = Stub<IEducationOrganizationCache>();
                edOrgCache.Stub(x => x.GetEducationOrganizationIdentifiers(0))
                    .IgnoreArguments()
                    .Return(new EducationOrganizationIdentifiers(4, "School", 1, 2, 3, 4));

                _securityRepository = new StubSecurityRepository();
            }

            protected override void Act()
            {
                var provider = new EdFiAuthorizationProvider(
                    _resourceAuthorizationMetadataProvider,
                    GetAuthorizationStrategies(),
                    _securityRepository);

                provider.AuthorizeSingleItem(_suppliedEdFiAuthorizationContext);
            }

            protected virtual void SetPrincipalAndStrategyAndContextValues()
            {
                SuppliedPrincipalClaim = "http://ed-fi.org/ods/identity/claims/domains/edFiTypes";
                SuppliedPrincipalAction = "http://ed-fi.org/ods/actions/read";

                SuppliedRequestedAction = "http://ed-fi.org/ods/actions/read";

                SuppliedResourceAuthorizationClaim = "http://ed-fi.org/ods/identity/claims/domains/edFiTypes";
                SuppliedResourceAuthorizationStrategy = "EdFiTypes";
            }

            //Claims that are needed for the resource in context.
            protected virtual IEnumerable<ResourceClaimAuthorizationStrategy> GetEdFiClaimsAuthorizationStrategyMetadata(
                string strategy, string claim)
            {
                return new List<ResourceClaimAuthorizationStrategy>
                {
                    new ResourceClaimAuthorizationStrategy
                    {
                        ClaimName = claim,
                        AuthorizationStrategy = strategy
                    }
                };
            }

            //The context to authorize.
            protected virtual EdFiAuthorizationContext GetEdFiAuthorizationContext(string action)
            {
                return GetEdFiAuthorizationContext(TestResource, action);
            }

            protected virtual EdFiAuthorizationContext GetEdFiAuthorizationContext(string resource, string action)
            {
                return new EdFiAuthorizationContext(GetClaimsPrincipal(),
                                                    resource,
                                                    action,
                                                    new object());
            }

            //The current principal
            protected virtual ClaimsPrincipal GetClaimsPrincipal()
            {
                return GetClaimsPrincipal(SuppliedPrincipalClaim, SuppliedPrincipalAction);
            }

            protected virtual ClaimsPrincipal GetClaimsPrincipal(string claim, string action)
            {
                var claimsPrincipal = Stub<ClaimsPrincipal>();

                var claims = new List<Claim>
                {
                    JsonClaimHelper.CreateClaim(claim,new EdFiResourceClaimValue(action))
                };

                claimsPrincipal.Expect(c => c.Claims).Return(claims);

                return claimsPrincipal;
            }

            protected virtual IEdFiAuthorizationStrategy[] GetAuthorizationStrategies()
            {
                _educationOrganizationHierarchyProvider = Stub<IEducationOrganizationHierarchyProvider>();
                _educationOrganizationHierarchyProvider.Stub(x => x.GetEducationOrganizationHierarchy())
                    .Return(new AdjacencyGraph<string, Edge<string>>());


                var edOrgCache = Stub<IEducationOrganizationCache>();
                edOrgCache.Stub(x => x.GetEducationOrganizationIdentifiers(0))
                    .IgnoreArguments()
                    .Return(new EducationOrganizationIdentifiers(4, "School", 1, 2, 3, 4));

                return new IEdFiAuthorizationStrategy[]
                {
                    new RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy<RelationshipsAuthorizationContextData>(
                        new ConcreteEducationOrganizationIdAuthorizationContextDataTransformer<RelationshipsAuthorizationContextData>(edOrgCache), 
                        null,
                        null,
                        null, 
                        _educationOrganizationHierarchyProvider,
                        null),
                    
                    new NoFurtherAuthorizationRequiredAuthorizationStrategy(),
                };
            }
        }

        public class When_authorizing_with_a_matching_resource_claim_and_action 
            : When_authorizing
        {
            protected override void SetPrincipalAndStrategyAndContextValues()
            {
                //Claims and Actions are the same...
                this.SuppliedPrincipalClaim = "http://ed-fi.org/ods/identity/claims/domains/edFiTypes";
                this.SuppliedResourceAuthorizationClaim = "http://ed-fi.org/ods/identity/claims/domains/edFiTypes";

                this.SuppliedPrincipalAction = "http://ed-fi.org/ods/actions/read";
                this.SuppliedRequestedAction = "http://ed-fi.org/ods/actions/read";

                this.SuppliedResourceAuthorizationStrategy = "NoFurtherAuthorizationRequired";
            }

            [Assert]
            public void Should_NOT_throw_exception()
            {
                ActualException.ShouldBeNull();
            }
        }

        public class When_authorizing_WITHOUT_a_matching_resource_claim 
            : When_authorizing
        {
            protected override void SetPrincipalAndStrategyAndContextValues()
            {
                //If the principal doesnt have the right claim for the resource it should fail.
                this.SuppliedPrincipalClaim = "http://ed-fi.org/ods/identity/claims/domains/edFiDescriptors";
                this.SuppliedResourceAuthorizationClaim = "http://ed-fi.org/ods/identity/claims/domains/edFiTypes";

                this.SuppliedPrincipalAction = "Not relevant for this test.";
                this.SuppliedRequestedAction = "Not relevant for this test.";
                this.SuppliedResourceAuthorizationStrategy = "Not relevant for this test.";
            }

            [Assert]
            public void Should_throw_exception()
            {
                ActualException.ShouldNotBeNull();
                ActualException.Message.ShouldContain("Are you missing a claim?");
            }
        }

        public class When_authorizing_with_a_matching_claim_but_without_a_matching_action 
            : When_authorizing
        {
            protected override void SetPrincipalAndStrategyAndContextValues()
            {
                //If the principal doesnt have the right claim for the resource it should fail.
                this.SuppliedPrincipalClaim = "http://ed-fi.org/ods/identity/claims/domains/edFiTypes";
                this.SuppliedResourceAuthorizationClaim = "http://ed-fi.org/ods/identity/claims/domains/edFiTypes";

                this.SuppliedPrincipalAction = "http://ed-fi.org/ods/actions/read";
                this.SuppliedRequestedAction = "http://ed-fi.org/ods/actions/create";

                this.SuppliedResourceAuthorizationStrategy = "Not relevant for this test.";
            }

            [Assert]
            public void Should_throw_exception_indicating_authorization_failed_for_the_requested_action()
            {
                ActualException.ShouldNotBeNull();
                ActualException.Message.ShouldEqual(
                    string.Format(
                        "Access to the resource could not be authorized for the requested action '{0}'.",
                        SuppliedRequestedAction));
            }
        }
    }

    public class Feature_Detecting_missing_or_undefined_authorization_strategies
    {
        // Feature constants
        private const string Resource1ClaimUri = @"http://CLAIMS/resource1";
        private const string Resource2ClaimUri = @"http://CLAIMS/resource2";
        private const string ReadActionUri = @"http://ACTIONS/read";

        // Feature artifacts
        public class UnusedAuthorizationStrategy : IEdFiAuthorizationStrategy
        {
            public bool SingleItemWasCalled { get; private set; }
            public bool FilteringWasCalled { get; private set; }

            public void AuthorizeSingleItem(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext)
            {
                SingleItemWasCalled = true;
            }

            public void ApplyAuthorizationFilters(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext, ParameterizedFilterBuilder filterBuilder)
            {
                FilteringWasCalled = true;
            }
        }

        #region Givens

        private static T Given_just_a_stub_for<T>()
            where T : class
        {
            return MockRepository.GenerateStub<T>();
        }

        private static EdFiAuthorizationContext 
            Given_an_authorization_context_for_a_request_to_read_a_resource_with_a_principal(
            string resourceClaimUri, ClaimsPrincipal principal)
        {
            return new EdFiAuthorizationContext(
                principal,
                resourceClaimUri,
                ReadActionUri,
                new object());
        }

        private static IResourceAuthorizationMetadataProvider
            Given_authorization_metadata_for_resource_claim_and_action_with_authorization_strategy(
            string resourceClaim,
            string actionUri,
            string authorizationStrategyName)
        {
            var authorizationMetadataProvider =
                MockRepository.GenerateStub<IResourceAuthorizationMetadataProvider>();
            
            authorizationMetadataProvider
                .Stub(x => x.GetResourceClaimAuthorizationStrategies(resourceClaim, actionUri))
                .Return(
                    (new List<ResourceClaimAuthorizationStrategy>
                    {
                        new ResourceClaimAuthorizationStrategy
                        {
                            ClaimName = resourceClaim,
                            AuthorizationStrategy = authorizationStrategyName
                        },
                    })
                        .ToList());

            return authorizationMetadataProvider;
        }

        private static ISecurityRepository Given_a_security_repository_returning_all_actions()
        {
            var securityRepository = MockRepository.GenerateStub<ISecurityRepository>();
            securityRepository.Stub(sr => sr.GetActionByName("Create"))
                              .Return(
                                  new Action
                                  {
                                      ActionId = 1,
                                      ActionName = "Create",
                                      ActionUri = "http://ACTIONS/create"
                                  });
            securityRepository.Stub(sr => sr.GetActionByName("Read"))
                              .Return(
                                  new Action
                                  {
                                      ActionId = 1,
                                      ActionName = "Read",
                                      ActionUri = "http://ACTIONS/read"
                                  });
            securityRepository.Stub(sr => sr.GetActionByName("Update"))
                              .Return(
                                  new Action
                                  {
                                      ActionId = 1,
                                      ActionName = "Update",
                                      ActionUri = "http://ACTIONS/update"
                                  });
            securityRepository.Stub(sr => sr.GetActionByName("Delete"))
                              .Return(
                                  new Action
                                  {
                                      ActionId = 1,
                                      ActionName = "Delete",
                                      ActionUri = "http://ACTIONS/delete"
                                  });

            return securityRepository;
        }

        private static IEdFiAuthorizationStrategy[] Given_a_collection_of_unused_authorization_strategies()
        {
            return new IEdFiAuthorizationStrategy[] { new UnusedAuthorizationStrategy() };
        }

        private static ClaimsPrincipal Given_a_ClaimsPrincipal_with_a_read_claim_for_resource(
            string resourceClaim)
        {
            var claimsIdentity =
                new ClaimsIdentity(
                    new[]
                    {
                        JsonClaimHelper.CreateClaim(
                            resourceClaim,
                            new EdFiResourceClaimValue(ReadActionUri))
                    });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            return claimsPrincipal;
        }

        #endregion

        public class When_no_matching_authorization_strategy_implementation_can_be_found 
            : TestFixtureBase
        {
            // Supplied values

            // Actual values

            protected override void Act()
            {
                // Execute code under test
                var provider = new EdFiAuthorizationProvider(
                    Given_authorization_metadata_for_resource_claim_and_action_with_authorization_strategy(
                        Resource1ClaimUri, ReadActionUri, "Missing"),
                    Given_a_collection_of_unused_authorization_strategies(),
                    Given_a_security_repository_returning_all_actions());

                // Request is for Resource Claim 1
                provider.AuthorizeSingleItem(
                    Given_an_authorization_context_for_a_request_to_read_a_resource_with_a_principal(Resource1ClaimUri,
                        Given_a_ClaimsPrincipal_with_a_read_claim_for_resource(Resource1ClaimUri)));
            }

            [Assert]
            public void Should_throw_exception_indicating_that_the_authorization_strategy_could_not_be_found()
            {
                ActualException.ShouldBeExceptionType<Exception>();
                ActualException.Message.ShouldContain("Missing");
            }
        }

        public class When_there_is_no_authorization_strategy_defined_in_the_metadata_for_the_matched_claim 
            : TestFixtureBase
        {
            // Supplied values

            // Actual values

            protected override void Act()
            {
                // Execute code under test

                var provider = new EdFiAuthorizationProvider(
                    Given_authorization_metadata_for_resource_claim_and_action_with_authorization_strategy(
                        Resource2ClaimUri, ReadActionUri, null),
                    Given_a_collection_of_unused_authorization_strategies(),
                    Given_a_security_repository_returning_all_actions());

                provider.AuthorizeSingleItem(
                    Given_an_authorization_context_for_a_request_to_read_a_resource_with_a_principal(Resource2ClaimUri, 
                        Given_a_ClaimsPrincipal_with_a_read_claim_for_resource(Resource2ClaimUri)));
            }

            [Assert]
            public void Should_throw_exception_indicating_that_no_authorization_strategy_was_defined_in_the_metadata()
            {
                ActualException.ShouldBeExceptionType<Exception>();
                ActualException.Message.ShouldContain("No authorization strategy was defined");
            }
        }
    }
}
