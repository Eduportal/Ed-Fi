using System.Linq;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Common.Context;
using EdFi.Common.Database;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Common.Security;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Security.Authorization.ContextDataProviders;
using EdFi.Ods.Security.AuthorizationStrategies.Relationships;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.Security._Installers;
using EdFi.Ods.Tests.EdFi.Ods.Security._Stubs;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.Utilities.CastleWindsor
{
    // Fake/Null Dependency needed for container testing.
    public class NullDatabaseConnectionStringProvider : IDatabaseConnectionStringProvider
    {
        public string GetConnectionString()
        {
            return null;
        }
    }

    public class SecurityComponentsInstallerTests
    {

        [TestFixture]
        public class When_registering_in_the_container : TestBase
        {
            private IWindsorContainer testContainer;

            [TestFixtureSetUp]
            public void Setup()
            {
                var factory = new InversionOfControlContainerFactory();
                testContainer = factory.CreateContainer(c =>
                {
                    c.Kernel.Resolver.AddSubResolver(new ArrayResolver(c.Kernel, true));
                });

                testContainer.AddFacility<TypedFactoryFacility>();

                testContainer.Register(Component
                    .For<IConfigValueProvider>()
                    .ImplementedBy<NameValueCollectionConfigValueProvider>());

                testContainer.Register(Component
                    .For<ICacheProvider>()
                    .ImplementedBy<MemoryCacheProvider>());

                testContainer.Register(Component
                .For<IDatabaseConnectionStringProvider>()
                .ImplementedBy<NullDatabaseConnectionStringProvider>());

                testContainer.Register(Component
                    .For<IContextStorage>()
                    .ImplementedBy<HashtableContextStorage>());

                testContainer.Register(Component
                    .For<IApiKeyContextProvider>()
                    .ImplementedBy<ApiKeyContextProvider>());

                testContainer.Register(Component
                    .For<ISecurityRepository>()
                    .ImplementedBy<StubSecurityRepository>());

                testContainer.Register(
                    Component
                        .For<ISessionFactory>()
                        .Instance(MockRepository.GenerateStub<ISessionFactory>()));

                testContainer.Install(new SecurityComponentsInstaller());
            }

            [Test]
            public void Should_resolve_the_relationship_based_authorization_context_providers_for_the_corresponding_entity_types()
            {
                var provider = testContainer.Resolve<IRelationshipsAuthorizationContextDataProvider<IStudent, RelationshipsAuthorizationContextData>>();

                provider.GetType().ShouldEqual(typeof(StudentRelationshipsAuthorizationContextDataProvider<RelationshipsAuthorizationContextData>));
            }

            [Test]
            public void Should_resolve_the_EdFi_authorization_provider_and_all_of_its_dependencies()
            {
                // These should not throw exceptions.
                var edFiAuthorizationProvider = testContainer.Resolve<IEdFiAuthorizationProvider>();
            }

            [Test]
            public void Should_resolve_the_EdFi_authorization_strategies_and_all_of_its_dependencies()
            {
                var expectedTypes =
                    (from t in typeof(SecurityComponentsInstaller).Assembly.GetTypes()
                     where !t.IsAbstract
                           && typeof(IEdFiAuthorizationStrategy).IsAssignableFrom(t)
                     orderby t.FullName
                     select t.FullName.TrimAt('`'))
                        .ToList();

                var edFiAuthorizationStrategies = testContainer.ResolveAll<IEdFiAuthorizationStrategy>();
                var actualTypes = edFiAuthorizationStrategies
                    .Select(x => x.GetType().FullName.TrimAt('`'))
                    .OrderBy(x => x)
                    .ToList();

                Assert.That(actualTypes, Is.EquivalentTo(expectedTypes));
            }
        }
    }
}
