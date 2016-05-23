using EdFi.Common.InversionOfControl;

using EdFi.Common.Security.Authorization;
using EdFi.Ods.Security.AuthorizationStrategies.Relationships;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.Authorization.Repositories
{
    using System;
    using System.Collections.Generic;

    using Castle.MicroKernel.Registration;
    using global::EdFi.Common.Security.Claims;
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Common.Repositories;
    using global::EdFi.Ods.Entities.NHibernate.StudentAggregate;
    using global::EdFi.Ods.Security.Authorization;
    using global::EdFi.Ods.Security.Authorization.Repositories;
    using global::EdFi.Ods.Security._Installers;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class SecurityComponentsInstallerTest
    {
        [TestFixture]
        [Ignore("Cannot be executed since SecurityComponentsInstaller doesn't install the decorators right now.")]
        public class When_resolving_authorization_decorators
        {
            private IGetEntityByKey<Student> resolvedIGetEntityByKey;
            private DecoratedGetEntityByKey<Student> DecoratedGetEntityByKeyInstance;

            [SetUp]
            public void SetUp()
            {
                var container = new WindsorContainerEx();
                container.Install(new SecurityComponentsInstaller());

                container.Register(Component
                    .For(typeof (IGetEntityByKey<>))
                    .ImplementedBy(typeof (DecoratedGetEntityByKey<>))
                    .OnCreate((kernel, item) => DecoratedGetEntityByKeyInstance = (DecoratedGetEntityByKey<Student>)item));

                container.Register(
                    Component
                        .For(typeof (IRelationshipsAuthorizationContextDataProvider<>))
                        .ImplementedBy(typeof (RelationshipsAuthorizationContextDataProviderStub<>)) //,
                    );

                this.resolvedIGetEntityByKey = container.Resolve<IGetEntityByKey<Student>>();
            }

            [Test]
            public void Resolved_object_should_be_the_decorator_()
            {
                this.resolvedIGetEntityByKey.ShouldBeType(typeof(GetEntityByKeyAuthorizationDecorator<Student>));
            }

            [Test]
            public void When_invoking_the_service_the_decorator_should_pass_control_to_the_decorated_object()
            {
                this.resolvedIGetEntityByKey.GetByKey(new Student());
                this.DecoratedGetEntityByKeyInstance.WasCalled.ShouldBeTrue();
            }
        }

        [TestFixture]
        [Ignore("Cannot be executed since SecurityComponentsInstaller doesn't install the decorators right now.")]
        public class When_installing_authorization_decorators
        {
            [Test]
            [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "The Authorization decorator must be the first registered service for type 'IGetEntityByKey'")]
            public void Should_throw_exception_if_authorization_decoratos_not_being_registed_first()
            {
                var container = new WindsorContainerEx();

                container.Register(Component
                    .For(typeof(IGetEntityByKey<>))
                    .ImplementedBy(typeof(DecoratedGetEntityByKey<>))
                  );

                container.Register(
                    Component
                        .For(typeof(IRelationshipsAuthorizationContextDataProvider<>))
                        .ImplementedBy(typeof(RelationshipsAuthorizationContextDataProviderStub<>)) //,
                    );


                container.Install(new SecurityComponentsInstaller());
            }
        }
    }

    class DecoratedGetEntityByKey<T> : IGetEntityByKey<T> where T : IDateVersionedEntity, IHasIdentifier
    {
        public bool WasCalled { get; set; }
        public T GetByKey(T specification)
        {
            this.WasCalled = true;
            return specification;
        }
    }

    class RelationshipsAuthorizationContextDataProviderStub<T> : IRelationshipsAuthorizationContextDataProvider<T, RelationshipsAuthorizationContextData>
    {
        public RelationshipsAuthorizationContextData GetContextData(T resource)
        {
            return new RelationshipsAuthorizationContextData();
        }

        public RelationshipsAuthorizationContextData GetContextData(object resource)
        {
            return new RelationshipsAuthorizationContextData();
        }

        /// <summary>
        /// Gets the properties that are relevant for relationship-based authorization.
        /// </summary>
        /// <returns>The names of the properties to be used for the authorization context.</returns>
        public string[] GetAuthorizationContextPropertyNames()
        {
            throw new NotImplementedException();
        }
    }
}