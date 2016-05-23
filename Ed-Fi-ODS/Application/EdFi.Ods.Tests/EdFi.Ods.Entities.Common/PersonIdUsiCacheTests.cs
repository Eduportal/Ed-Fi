using System;
using System.Runtime.Caching;
using EdFi.Common.Caching;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.Common.IdentityValueMappers;
using EdFi.Ods.Entities.Common.Providers;
using EdFi.Ods.Tests._Bases;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Entities.Common
{
    public class When_getting_usi_and_id_for_non_cached_unique_id_and_both_values_are_returned_by_usi_value_mapper : TestFixtureBase
    {
        // Supplied values
        private PersonIdentifiersValueMap suppliedUsiValueMap;

        // Actual values
        private int actualUsi;
        private Guid actualId;

        // External dependencies
        private IUniqueIdToUsiValueMapper usiValueMapper;
        private IUniqueIdToIdValueMapper idValueMapper;
        private IEdFiOdsInstanceIdentificationProvider edfiOdsInstanceIdentificationProvider;

        protected override void Arrange()
        {
            this.edfiOdsInstanceIdentificationProvider = this.Stub<IEdFiOdsInstanceIdentificationProvider>();
            this.idValueMapper = this.Stub<IUniqueIdToIdValueMapper>();
            this.usiValueMapper = this.Stub<IUniqueIdToUsiValueMapper>();

            this.suppliedUsiValueMap = new PersonIdentifiersValueMap
            {
                UniqueId = Guid.NewGuid().ToString(),
                Usi = 10,
                Id = Guid.NewGuid(),  // Id is also provided by the USI value mapper
            };

            this.usiValueMapper.Stub(x => x.GetUsi(string.Empty, string.Empty))
                .IgnoreArguments()
                .Return(this.suppliedUsiValueMap);
        }

        protected override void Act()
        {
            var memoryCacheProvider = new MemoryCacheProvider { MemoryCache = new MemoryCache("IsolatedForUnitTest") };

            var idCache = new PersonUniqueIdToIdCache(
                memoryCacheProvider,
                this.edfiOdsInstanceIdentificationProvider,
                this.idValueMapper);

            var usiCache = new PersonUniqueIdToUsiCache(
                memoryCacheProvider,
                this.edfiOdsInstanceIdentificationProvider,
                this.usiValueMapper);

            PersonUniqueIdToUsiCache.GetCache = () => usiCache;

            this.actualUsi = usiCache.GetUsi("Staff", this.suppliedUsiValueMap.UniqueId);
            this.actualId = idCache.GetId("Staff", this.suppliedUsiValueMap.UniqueId);
        }

        [Assert]
        public void Should_call_usi_value_mapper_for_the_usi()
        {
            this.usiValueMapper.AssertWasCalled(x => x.GetUsi("Staff", this.suppliedUsiValueMap.UniqueId));
        }

        [Assert]
        public void Should_have_opportunistically_cached_the_id_from_the_usi_value_mapper()
        {
            // Value was cached opportunistically by initial call, so no call necessary
            this.idValueMapper.AssertWasNotCalled(x => x.GetId("Staff", this.suppliedUsiValueMap.UniqueId));
        }

        [Assert]
        public void Should_return_correct_values_from_cache()
        {
            this.actualUsi.ShouldEqual(this.suppliedUsiValueMap.Usi);
            this.actualId.ShouldEqual(this.suppliedUsiValueMap.Id);
        }
    }

    public class When_getting_id_and_usi_for_non_cached_unique_id_and_both_values_are_returned_by_id_value_mapper : TestFixtureBase
    {
        // Supplied values
        private PersonIdentifiersValueMap suppliedIdValueMap;

        // Actual values
        private int actualUsi;
        private Guid actualId;

        // External dependencies
        private IUniqueIdToUsiValueMapper usiValueMapper;
        private IUniqueIdToIdValueMapper idValueMapper;
        private FakeEdFiOdsInstanceIdentificationProvider edfiOdsInstanceIdentificationProvider;
        private bool actualOpportunisticallyCachedUsiForFirstOds;
        private int actualUsiReturnedForSecondOdsContext;
        private PersonIdentifiersValueMap suppliedUsiValueMap;

        protected override void Arrange()
        {
            this.edfiOdsInstanceIdentificationProvider = new FakeEdFiOdsInstanceIdentificationProvider();
            this.idValueMapper = this.Stub<IUniqueIdToIdValueMapper>();
            this.usiValueMapper = this.Stub<IUniqueIdToUsiValueMapper>();

            this.suppliedIdValueMap = new PersonIdentifiersValueMap
            {
                UniqueId = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                Usi = 10,  // USI is also provided by the id value mapper
            };

            this.idValueMapper.Stub(x => x.GetId(string.Empty, string.Empty))
                .IgnoreArguments()
                .Return(this.suppliedIdValueMap);

            this.suppliedUsiValueMap = new PersonIdentifiersValueMap
            {
                UniqueId = this.suppliedIdValueMap.UniqueId, // Same UniqueId as above
                Id = this.suppliedIdValueMap.Id,             // Same Id as above
                Usi = 100,                              // USI is also provided, but different from above (simulating different ODS)
            };

            this.usiValueMapper.Stub(x => x.GetUsi("Staff", this.suppliedUsiValueMap.UniqueId)).Return(this.suppliedUsiValueMap);
        }

        protected override void Act()
        {
            var memoryCacheProvider = new MemoryCacheProvider { MemoryCache = new MemoryCache("IsolatedForUnitTest") };

            var idCache = new PersonUniqueIdToIdCache(
                memoryCacheProvider,
                this.edfiOdsInstanceIdentificationProvider,
                this.idValueMapper);

            var usiCache = new PersonUniqueIdToUsiCache(
                memoryCacheProvider,
                this.edfiOdsInstanceIdentificationProvider,
                this.usiValueMapper);

            this.actualId = idCache.GetId("Staff", this.suppliedIdValueMap.UniqueId);
            this.actualUsi = usiCache.GetUsi("Staff", this.suppliedIdValueMap.UniqueId);

            // Was the value was cached opportunistically by initial call to GetId?
            this.actualOpportunisticallyCachedUsiForFirstOds = true;
            try { this.usiValueMapper.AssertWasNotCalled(x => x.GetUsi("Staff", this.suppliedIdValueMap.UniqueId)); }
            catch (Exception) { this.actualOpportunisticallyCachedUsiForFirstOds = false; }

            // Change ODS connection instance
            this.edfiOdsInstanceIdentificationProvider.SetInstanceIdentification(2);
            this.actualUsiReturnedForSecondOdsContext = usiCache.GetUsi("Staff", this.suppliedIdValueMap.UniqueId);
        }

        [Assert]
        public void Should_call_id_value_mapper_for_the_id()
        {
            this.idValueMapper.AssertWasCalled(x => x.GetId("Staff", this.suppliedIdValueMap.UniqueId));
        }

        [Assert]
        public void Should_have_opportunistically_cached_the_usi_from_the_id_value_mapper()
        {
            // Value was cached opportunistically by initial call, so no call necessary
            this.actualOpportunisticallyCachedUsiForFirstOds.ShouldBeTrue();
        }

        [Assert]
        public void Should_return_correct_values_from_cache_for_first_ODS()
        {
            this.actualUsi.ShouldEqual(this.suppliedIdValueMap.Usi);
            this.actualId.ShouldEqual(this.suppliedIdValueMap.Id);
        }

        [Assert]
        public void Should_return_correct_value_from_cache_for_second_ODS()
        {
            this.actualUsiReturnedForSecondOdsContext.ShouldEqual(this.suppliedUsiValueMap.Usi);
        }
    }

    public class When_getting_usi_and_id_for_non_cached_unique_id_and_each_value_mapper_only_returns_the_requested_value : TestFixtureBase
    {
        // Supplied values
        private PersonIdentifiersValueMap suppliedUsiValueMap;
        private PersonIdentifiersValueMap suppliedIdValueMap;

        // Actual values
        private int actualUsi;
        private Guid actualId;

        // External dependencies
        private IUniqueIdToUsiValueMapper usiValueMapper;
        private IUniqueIdToIdValueMapper idValueMapper;
        private IEdFiOdsInstanceIdentificationProvider edfiOdsInstanceIdentificationProvider;

        protected override void Arrange()
        {
            this.edfiOdsInstanceIdentificationProvider = this.Stub<IEdFiOdsInstanceIdentificationProvider>();
            this.idValueMapper = this.Stub<IUniqueIdToIdValueMapper>();
            this.usiValueMapper = this.Stub<IUniqueIdToUsiValueMapper>();

            // usi value mapper
            this.suppliedUsiValueMap = new PersonIdentifiersValueMap
            {
                UniqueId = Guid.NewGuid().ToString(),
                Usi = 10,
            };

            this.usiValueMapper.Stub(x => x.GetUsi(string.Empty, string.Empty))
                .IgnoreArguments()
                .Return(this.suppliedUsiValueMap);

            // id value mapper
            this.suppliedIdValueMap = new PersonIdentifiersValueMap
            {
                UniqueId = this.suppliedUsiValueMap.UniqueId, // Same uniqueId
                Id = Guid.NewGuid(),
            };

            this.idValueMapper.Stub(x => x.GetId(string.Empty, string.Empty))
                .IgnoreArguments()
                .Return(this.suppliedIdValueMap);
        }

        protected override void Act()
        {
            var memoryCacheProvider = new MemoryCacheProvider { MemoryCache = new MemoryCache("IsolatedForUnitTest") };

            var idCache = new PersonUniqueIdToIdCache(
                memoryCacheProvider,
                this.edfiOdsInstanceIdentificationProvider,
                this.idValueMapper);

            var usiCache = new PersonUniqueIdToUsiCache(
                memoryCacheProvider,
                this.edfiOdsInstanceIdentificationProvider,
                this.usiValueMapper);

            this.actualUsi = usiCache.GetUsi("Staff", this.suppliedUsiValueMap.UniqueId);
            this.actualId = idCache.GetId("Staff", this.suppliedUsiValueMap.UniqueId);
        }

        [Assert]
        public void Should_call_usi_value_mapper_for_the_usi()
        {
            this.usiValueMapper.AssertWasCalled(x => x.GetUsi("Staff", this.suppliedUsiValueMap.UniqueId));
        }

        [Assert]
        public void Should_call_the_id_value_mapper_for_the_id()
        {
            this.idValueMapper.AssertWasCalled(x => x.GetId("Staff", this.suppliedIdValueMap.UniqueId));
        }

        [Assert]
        public void Should_return_correct_values_from_cache()
        {
            this.actualUsi.ShouldEqual(this.suppliedUsiValueMap.Usi);
            this.actualId.ShouldEqual(this.suppliedIdValueMap.Id);
        }
    }

    public class When_getting_usi_for_nullable_property_with_empty_or_null_or_non_existing_uniqueids : TestFixtureBase
    {
        // Actual results
        private int? actualUsiFromEmpty;
        private int? actualUsiFromNull;
        private int? actualUsiFromNonExisting;

        // External dependencies
        private IEdFiOdsInstanceIdentificationProvider edFiOdsInstanceIdentificationProvider;
        private IUniqueIdToUsiValueMapper usiValueMapper;
        private IUniqueIdToIdValueMapper idValueMapper;

        protected override void Arrange()
        {
            this.edFiOdsInstanceIdentificationProvider = this.Stub<IEdFiOdsInstanceIdentificationProvider>();

            this.usiValueMapper = this.mocks.StrictMock<IUniqueIdToUsiValueMapper>();
            SetupResult.For(this.usiValueMapper.GetUsi(null, null)).IgnoreArguments().Return(new PersonIdentifiersValueMap());

            this.idValueMapper = this.mocks.StrictMock<IUniqueIdToIdValueMapper>();
            SetupResult.For(this.idValueMapper.GetId(null, null)).IgnoreArguments().Return(new PersonIdentifiersValueMap());
        }

        protected override void Act()
        {
            var memoryCacheProvider = new MemoryCacheProvider { MemoryCache = new MemoryCache("IsolatedForUnitTest") };

            var usiCache = new PersonUniqueIdToUsiCache(
                memoryCacheProvider,
                this.edFiOdsInstanceIdentificationProvider,
                this.usiValueMapper);

            this.actualUsiFromEmpty = usiCache.GetUsiNullable("Staff", string.Empty);
            this.actualUsiFromNull = usiCache.GetUsiNullable("Staff", null);
            this.actualUsiFromNonExisting = usiCache.GetUsiNullable("Staff", "NotThere");
        }

        [Assert]
        public void Should_return_nulls_instead_of_zeros()
        {
            this.actualUsiFromEmpty.ShouldBeNull();
            this.actualUsiFromNull.ShouldBeNull();
            this.actualUsiFromNonExisting.ShouldBeNull();
        }
    }

    public class When_getting_usi_for_non_nullable_property_with_empty_or_null_or_non_existing_uniqueids : TestFixtureBase
    {
        // Actual results
        private int? actualUsiFromEmpty;
        private int? actualUsiFromNull;
        private int? actualUsiFromNonExisting;

        // External dependencies
        private IEdFiOdsInstanceIdentificationProvider edFiOdsInstanceIdentificationProvider;
        private IUniqueIdToUsiValueMapper usiValueMapper;
        private IUniqueIdToIdValueMapper idValueMapper;

        protected override void Arrange()
        {
            this.edFiOdsInstanceIdentificationProvider = this.Stub<IEdFiOdsInstanceIdentificationProvider>();

            this.usiValueMapper = this.mocks.StrictMock<IUniqueIdToUsiValueMapper>();
            SetupResult.For(this.usiValueMapper.GetUsi(null, null)).IgnoreArguments().Return(new PersonIdentifiersValueMap());

            this.idValueMapper = this.mocks.StrictMock<IUniqueIdToIdValueMapper>();
            SetupResult.For(this.idValueMapper.GetId(null, null)).IgnoreArguments().Return(new PersonIdentifiersValueMap());
        }

        protected override void Act()
        {
            var memoryCacheProvider = new MemoryCacheProvider { MemoryCache = new MemoryCache("IsolatedForUnitTest") };

            var usiCache = new PersonUniqueIdToUsiCache(
                memoryCacheProvider,
                this.edFiOdsInstanceIdentificationProvider,
                this.usiValueMapper);

            this.actualUsiFromEmpty = usiCache.GetUsi("Staff", string.Empty);
            this.actualUsiFromNull = usiCache.GetUsi("Staff", null);
            this.actualUsiFromNonExisting = usiCache.GetUsi("Staff", "NotThere");
        }

        [Assert]
        public void Should_return_zeros_instead_of_nulls()
        {
            this.actualUsiFromEmpty.ShouldEqual(0);
            this.actualUsiFromNull.ShouldEqual(0);
            this.actualUsiFromNonExisting.ShouldEqual(0);
        }
    }

    class FakeEdFiOdsInstanceIdentificationProvider : IEdFiOdsInstanceIdentificationProvider
    {
        private int value;

        public void SetInstanceIdentification(int value)
        {
            this.value = value;
        }

        public int GetInstanceIdentification()
        {
            return this.value;
        }
    }

    public class When_getting_uniqueId_by_usis_from_different_ODS_instances : TestFixtureBase
    {
        // Actual values
        private string actualUniqueIdFromOds1;
        private string actualUniqueIdFromOds2;

        // External dependencies
        private FakeEdFiOdsInstanceIdentificationProvider edFiOdsInstanceIdentificationProvider;
        private IUniqueIdToUsiValueMapper usiValueMapper;
        private const IUniqueIdToIdValueMapper idValueMapper = null;

        protected override void Arrange()
        {
            this.edFiOdsInstanceIdentificationProvider = new FakeEdFiOdsInstanceIdentificationProvider();

            // USI value mapper gets call twice during Act step, with first value on ODS instance 1, and second on ODS instance 2
            this.usiValueMapper = this.mocks.StrictMock<IUniqueIdToUsiValueMapper>();
            Expect.Call(this.usiValueMapper.GetUniqueId("Staff", 11)).Return(new PersonIdentifiersValueMap { UniqueId = "ABC123", Usi = 11 });
            Expect.Call(this.usiValueMapper.GetUniqueId("Staff", 11)).Return(new PersonIdentifiersValueMap { UniqueId = "CDE234", Usi = 11 });
        }

        protected override void Act()
        {
            var memoryCacheProvider = new MemoryCacheProvider { MemoryCache = new MemoryCache("IsolatedForUnitTest") };

            var usiCache = new PersonUniqueIdToUsiCache(
                memoryCacheProvider,
                this.edFiOdsInstanceIdentificationProvider,
                this.usiValueMapper);

            // Populate cache value from ODS instance 1
            this.edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(1);
            usiCache.GetUniqueId("Staff", 11);

            // Populate cache value from ODS instance 2
            this.edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(2);
            usiCache.GetUniqueId("Staff", 11);

            // Get cached value for ODS instance 1
            this.edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(1);
            this.actualUniqueIdFromOds1 = usiCache.GetUniqueId("Staff", 11);

            // Get cached value for ODS instance 2
            this.edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(2);
            this.actualUniqueIdFromOds2 = usiCache.GetUniqueId("Staff", 11);
        }

        [Assert]
        public void Should_retrieve_uniqueIds_from_cache_based_on_the_ODS_context()
        {
            this.actualUniqueIdFromOds1.ShouldEqual("ABC123");
            this.actualUniqueIdFromOds2.ShouldEqual("CDE234");
        }
    }

    public class When_getting_usis_from_different_ODS_instances_and_id_value_mapper_returns_all_3_identifiers : TestFixtureBase
    {
        // Supplied values
        private Guid suppliedIdForUniqueIdABC123;

        // Actual values
        private int actualUsiFromOds1;
        private int actualUsiFromOds2;

        // External dependencies
        private FakeEdFiOdsInstanceIdentificationProvider edFiOdsInstanceIdentificationProvider;
        private IUniqueIdToUsiValueMapper usiValueMapper;
        private IUniqueIdToIdValueMapper idValueMapper;
        private Guid actualIdFromOds1;
        private Guid actualIdFromOds2;

        protected override void Arrange()
        {
            edFiOdsInstanceIdentificationProvider = new FakeEdFiOdsInstanceIdentificationProvider();

            suppliedIdForUniqueIdABC123 = Guid.NewGuid();

            usiValueMapper = mocks.StrictMock<IUniqueIdToUsiValueMapper>();
            
            Expect.Call(usiValueMapper.GetUsi("Staff", "ABC123"))
                .Return(
                    new PersonIdentifiersValueMap
                    {
                        UniqueId = "ABC123", 
                        Usi = 11, 
                        Id = suppliedIdForUniqueIdABC123 
                    });
            
            Expect.Call(usiValueMapper.GetUsi("Staff", "ABC123"))
                .Return(
                    new PersonIdentifiersValueMap
                    {
                        UniqueId = "ABC123", 
                        Usi = 22, 
                        Id = suppliedIdForUniqueIdABC123
                    });

            idValueMapper = Stub<IUniqueIdToIdValueMapper>();
        }

        protected override void Act()
        {
            var memoryCacheProvider = new MemoryCacheProvider {MemoryCache = new MemoryCache("IsolatedForUnitTest")};

            var idCache = new PersonUniqueIdToIdCache(
                memoryCacheProvider, 
                edFiOdsInstanceIdentificationProvider,
                idValueMapper);

            var usiCache = new PersonUniqueIdToUsiCache(
                memoryCacheProvider,
                edFiOdsInstanceIdentificationProvider,
                usiValueMapper);
            
            // Populate value from ODS instance 1
            edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(1);
            usiCache.GetUsi("Staff", "ABC123");

            // Populate value from ODS instance 2
            edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(2);
            usiCache.GetUsi("Staff", "ABC123");

            // ODS instance 1
            edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(1);
            actualUsiFromOds1 = usiCache.GetUsi("Staff", "ABC123");
            actualIdFromOds1 = idCache.GetId("Staff", "ABC123");

            // ODS instance 2
            edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(2);
            actualUsiFromOds2 = usiCache.GetUsi("Staff", "ABC123");
            actualIdFromOds2 = idCache.GetId("Staff", "ABC123");
        }

        [Assert]
        public void Should_retrieve_USI_value_from_cache_based_on_ODS_instances_in_context()
        {
            actualUsiFromOds1.ShouldEqual(11);
            actualUsiFromOds2.ShouldEqual(22);
        }

        [Assert]
        public void Should_opportunistically_cache_the_Id_after_the_first_call()
        {
            idValueMapper.AssertWasNotCalled(x => x.GetId("Staff", "ABC123"));
        }

        [Assert]
        public void Should_return_the_same_Id_independent_of_ODS_instance()
        {
            actualIdFromOds1.ShouldEqual(suppliedIdForUniqueIdABC123);
            actualIdFromOds2.ShouldEqual(suppliedIdForUniqueIdABC123);
        }
    }

    public class When_getting_usis_from_different_ODS_instances_and_id_value_mapper_does_not_provide_the_id : TestFixtureBase
    {
        // Supplied values
        private Guid suppliedIdForUniqueIdABC123;

        // Actual values
        private int actualUsiFromOds1;
        private int actualUsiFromOds2;

        // External dependencies
        private FakeEdFiOdsInstanceIdentificationProvider edFiOdsInstanceIdentificationProvider;
        private IUniqueIdToUsiValueMapper usiValueMapper;
        private IUniqueIdToIdValueMapper idValueMapper;
        private Guid actualIdFromOds1;
        private Guid actualIdFromOds2;

        protected override void Arrange()
        {
            edFiOdsInstanceIdentificationProvider = new FakeEdFiOdsInstanceIdentificationProvider();

            suppliedIdForUniqueIdABC123 = Guid.NewGuid();

            // USI value mapper gets call twice during Act step, with first value on ODS instance 1, and second on ODS instance 2
            usiValueMapper = mocks.StrictMock<IUniqueIdToUsiValueMapper>();
            Expect.Call(usiValueMapper.GetUsi("Staff", "ABC123"))
                .Return(new PersonIdentifiersValueMap { UniqueId = "ABC123", Usi = 11 });
            Expect.Call(usiValueMapper.GetUsi("Staff", "ABC123"))
                .Return(new PersonIdentifiersValueMap { UniqueId = "ABC123", Usi = 22 });

            idValueMapper = mocks.StrictMock<IUniqueIdToIdValueMapper>();
            Expect.Call(idValueMapper.GetId("Staff", "ABC123"))
                .Return(new PersonIdentifiersValueMap { UniqueId = "ABC123", Id = suppliedIdForUniqueIdABC123 });
        }

        protected override void Act()
        {
            var memoryCacheProvider = new MemoryCacheProvider { MemoryCache = new MemoryCache("IsolatedForUnitTest") };

            var idCache = new PersonUniqueIdToIdCache(
                memoryCacheProvider,
                edFiOdsInstanceIdentificationProvider,
                idValueMapper);

            var usiCache = new PersonUniqueIdToUsiCache(
                memoryCacheProvider,
                edFiOdsInstanceIdentificationProvider,
                usiValueMapper);
            
            // Populate cached value from ODS instance 1
            edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(1);
            usiCache.GetUsi("Staff", "ABC123");

            // Populate cached value from ODS instance 2
            edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(2);
            usiCache.GetUsi("Staff", "ABC123");

            // Get cached values from ODS instance 1
            edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(1);
            actualUsiFromOds1 = usiCache.GetUsi("Staff", "ABC123");
            actualIdFromOds1 = idCache.GetId("Staff", "ABC123");

            // Get cached values from ODS instance 2
            edFiOdsInstanceIdentificationProvider.SetInstanceIdentification(2);
            actualUsiFromOds2 = usiCache.GetUsi("Staff", "ABC123");
            actualIdFromOds2 = idCache.GetId("Staff", "ABC123");
        }

        [Assert]
        public void Should_retrieve_USI_value_from_cache_based_on_ODS_instances_in_context()
        {
            actualUsiFromOds1.ShouldEqual(11);
            actualUsiFromOds2.ShouldEqual(22);
        }

        [Assert]
        public void Should_return_the_same_Id_independent_of_ODS_instance()
        {
            actualIdFromOds1.ShouldEqual(suppliedIdForUniqueIdABC123);
            actualIdFromOds2.ShouldEqual(suppliedIdForUniqueIdABC123);
        }
    }

    public class When_getting_UniqueId_by_id : TestFixtureBase
    {
        // Supplied values
        private Guid suppliedId;

        // Actual values
        private string actualFirstValue;
        private string actualSecondValue;
        private Exception actualExceptionOnSecondRequestForValue;

        // External dependencies
        private IEdFiOdsInstanceIdentificationProvider edFiOdsInstanceIdentificationProvider;
        private const IUniqueIdToUsiValueMapper usiValueMapper = null;
        private IUniqueIdToIdValueMapper idValueMapper;

        protected override void Arrange()
        {
            this.edFiOdsInstanceIdentificationProvider = this.Stub<IEdFiOdsInstanceIdentificationProvider>();

            this.suppliedId = Guid.NewGuid();

            this.idValueMapper = this.mocks.StrictMock<IUniqueIdToIdValueMapper>();
            Expect.Call(this.idValueMapper.GetUniqueId("Staff", this.suppliedId)).Return(new PersonIdentifiersValueMap { Id = this.suppliedId, UniqueId = "ABC123" });
        }

        protected override void Act()
        {
            var memoryCacheProvider = new MemoryCacheProvider { MemoryCache = new MemoryCache("IsolatedForUnitTest") };

            var idCache = new PersonUniqueIdToIdCache(
                memoryCacheProvider,
                this.edFiOdsInstanceIdentificationProvider,
                this.idValueMapper);

            this.actualFirstValue = idCache.GetUniqueId("Staff", this.suppliedId);

            // Based on Rhino expecation, this will fail if value mapper is called twice by the cache
            try { this.actualSecondValue = idCache.GetUniqueId("Staff", this.suppliedId); }
            catch (Exception ex) { this.actualExceptionOnSecondRequestForValue = ex; }
        }

        [Assert]
        public void Should_return_uniqueId_value()
        {
            this.actualFirstValue.ShouldEqual("ABC123");
            this.actualSecondValue.ShouldEqual("ABC123");
        }

        [Assert]
        public void Should_not_call_value_mapper_a_second_time_for_same_value()
        {
            this.actualExceptionOnSecondRequestForValue.ShouldBeNull();
        }
    }
}