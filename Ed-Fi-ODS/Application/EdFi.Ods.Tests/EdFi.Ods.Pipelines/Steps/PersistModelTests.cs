using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Tests.EdFi.Ods.Pipelines.Steps
{
    using System;

    using global::EdFi.Ods.Api.Common;
    using global::EdFi.Ods.Api.Models.Resources.Account;
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Pipelines.Put;
    using global::EdFi.Ods.Pipelines.Steps;
    using global::EdFi.Ods.Tests.EdFi.Ods.Common._Stubs.Repositories;
    using global::EdFi.Ods.Tests._Bases;
    using global::EdFi.Ods.Tests._Builders;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    public class PersistModelTests
    {
        public class AccountResource : IHasETag
        {
            public string ETag
            {
                get { return "abcd"; }
                set { }
            }
        }
        
        public class AccountEntity : IDateVersionedEntity, IHasIdentifier
        {
            public DateTime LastModifiedDate
            {
                get { return DateTime.Now; }
                set {  }
            }

            public Guid Id
            {
                get { return Guid.NewGuid(); }
                set {  }
            }
        }

        [TestFixture]
        public class When_resource_is_neither_updated_nor_created : TestBase
        {
            private readonly PutResult _putResult = new PutResult();

            [TestFixtureSetUp]
            public void Setup()
            {
                var resource = new AccountResource();
                var persistentModel = new AccountEntity();
                var context = new PutContext<AccountResource, AccountEntity>(resource, new ValidationState()) {PersistentModel = persistentModel};

                var eTagProvider = this.Stub<IETagProvider>();
                StubRepository<AccountEntity> repository = New.StubRepository<AccountEntity>()
                                                        .ResourceIsNeverCreatedOrModified;

                var step = new PersistEntityModel<PutContext<AccountResource, AccountEntity>, PutResult, AccountResource, AccountEntity>((IUpsertEntity<AccountEntity>)repository, eTagProvider);
                step.Execute(context, this._putResult);
            }

            [Test]
            public void Should_still_indicate_resource_was_persisted()
            {
                this._putResult.ResourceWasPersisted.ShouldBeTrue();
            }

            [Test]
            public void Should_indicate_resource_was_not_created()
            {
                this._putResult.ResourceWasCreated.ShouldBeFalse();
            }

            [Test]
            public void Should_indicate_resource_was_not_updated()
            {
                this._putResult.ResourceWasUpdated.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_resource_is_created : TestBase
        {
            private readonly PutResult _putResult = new PutResult();

            [TestFixtureSetUp]
            public void Setup()
            {
                var resource = new AccountResource();
                var persistentModel = new AccountEntity();
                var context = new PutContext<AccountResource, AccountEntity>(resource, new ValidationState()) { PersistentModel = persistentModel };

                var eTagProvider = this.Stub<IETagProvider>();
                StubRepository<AccountEntity> repository = New.StubRepository<AccountEntity>()
                                                        .ResourceIsAlwaysCreated;

                var step = new PersistEntityModel<PutContext<AccountResource, AccountEntity>, PutResult, AccountResource, AccountEntity>((IUpsertEntity<AccountEntity>) repository, eTagProvider);
                step.Execute(context, this._putResult);
            }

            [Test]
            public void Should_still_indicate_resource_was_persisted()
            {
                this._putResult.ResourceWasPersisted.ShouldBeTrue();
            }

            [Test]
            public void Should_indicate_resource_was_created()
            {
                this._putResult.ResourceWasCreated.ShouldBeTrue();
            }

            [Test]
            public void Should_indicate_resource_was_not_updated()
            {
                this._putResult.ResourceWasUpdated.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_resource_is_modified : TestBase
        {
            private readonly PutResult _putResult = new PutResult();

            [TestFixtureSetUp]
            public void Setup()
            {
                var resource = new AccountResource();
                var persistentModel = new AccountEntity();
                var context = new PutContext<AccountResource, AccountEntity>(resource, new ValidationState()) { PersistentModel = persistentModel };

                var eTagProvider = this.Stub<IETagProvider>();
                StubRepository<AccountEntity> repository = New.StubRepository<AccountEntity>()
                                                        .ResourceIsAlwaysModified;

                var step = new PersistEntityModel<PutContext<AccountResource, AccountEntity>, PutResult, AccountResource, AccountEntity>((IUpsertEntity<AccountEntity>) repository, eTagProvider);
                step.Execute(context, this._putResult);
            }

            [Test]
            public void Should_still_indicate_resource_was_persisted()
            {
                this._putResult.ResourceWasPersisted.ShouldBeTrue();
            }

            [Test]
            public void Should_indicate_resource_was_not_created()
            {
                this._putResult.ResourceWasCreated.ShouldBeFalse();
            }

            [Test]
            public void Should_indicate_resource_was_updated()
            {
                this._putResult.ResourceWasUpdated.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_the_respository_throws_an_exception : TestBase
        {
            private readonly PutResult _putResult = new PutResult();

            [TestFixtureSetUp]
            public void Setup()
            {
                var resource = new AccountResource();
                var persistentModel = new AccountEntity();
                var context = new PutContext<AccountResource, AccountEntity>(resource, new ValidationState()) { PersistentModel = persistentModel };

                var eTagProvider = this.Stub<IETagProvider>();
                StubRepository<AccountEntity> repository = New.StubRepository<AccountEntity>()
                                                        .OnUpsertThrow(new Exception());

                var step = new PersistEntityModel<PutContext<AccountResource, AccountEntity>, PutResult, AccountResource, AccountEntity>((IUpsertEntity<AccountEntity>)repository, eTagProvider);
                step.Execute(context, this._putResult);
            }

            [Test]
            public void Should_not_indicate_resource_was_persisted()
            {
                this._putResult.ResourceWasPersisted.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_etag_provider_throws_an_exception : TestBase
        {
            private readonly PutResult _putResult = new PutResult();

            [TestFixtureSetUp]
            public void Setup()
            {
                var resource = new AccountResource();
                var persistentModel = new AccountEntity();
                var context = new PutContext<AccountResource, AccountEntity>(resource, new ValidationState()) { PersistentModel = persistentModel };

                var eTagProvider = this.Stub<IETagProvider>();
                eTagProvider.Stub(x => x.GetETag(null)).IgnoreArguments().Throw(new Exception("Some Fun Exception"));

                StubRepository<AccountEntity> repository = New.StubRepository<AccountEntity>()
                                                        .ResourceIsNeverCreatedOrModified;

                var step = new PersistEntityModel<PutContext<AccountResource, AccountEntity>, PutResult, AccountResource, AccountEntity>((IUpsertEntity<AccountEntity>)repository, eTagProvider);
                step.Execute(context, this._putResult);
            }

            [Test]
            public void Should_include_exception_in_result()
            {
                this._putResult.Exception.Message.ShouldEqual("Some Fun Exception");
            }

            [Test]
            public void Should_indicate_resource_was_persisted()
            {
                this._putResult.ResourceWasPersisted.ShouldBeTrue();
            }
        }
    }
}