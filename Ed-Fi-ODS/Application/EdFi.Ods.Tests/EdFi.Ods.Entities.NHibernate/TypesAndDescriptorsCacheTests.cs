using Castle.MicroKernel.Registration;
using EdFi.Common.Configuration;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Common.Utils;
using EdFi.Ods.Entities.NHibernate;
using log4net;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Common.Caching;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.Common.Providers;

namespace EdFi.Ods.Tests.EdFi.Ods.Entities.NHibernate
{
    [TestFixture]
    public class TypesAndDescriptorsCacheTests
    {
        protected const string TestDescriptorName = "AcademicSubjectDescriptor";
        protected const string DefaultNamespacePrefix = "http://www.ed-fi.org";
        protected DescriptorLookup TestDescriptorNormal = new DescriptorLookup { Id = 100, CodeValue = "110", Namespace = DefaultNamespacePrefix + "/Descriptor/AcademicSubjectDescriptor.xml", DescriptorName = TestDescriptorName };
        protected DescriptorLookup TestDescriptorCustom = new DescriptorLookup { Id = 200, CodeValue = "110", Namespace = "http://www.changetest.org/Descriptor/AcademicSubjectDescriptor.xml", DescriptorName = TestDescriptorName };
        protected DescriptorLookup TestDescriptorCustomNotIncluded = new DescriptorLookup { Id = 300, CodeValue = "210", Namespace = "http://www.changetest.org/Descriptor/AcademicSubjectDescriptor.xml", DescriptorName = TestDescriptorName };

        protected const string TestTypeName = "AcademicHonorCategoryType";
        protected TypeLookup TestType = new TypeLookup { Id = 10, ShortDescription = "Attendance Award", TypeName = TestTypeName } ;

        protected ITypeLookupProvider MockTypeCacheDataProvider { get; set; }
        protected IDescriptorLookupProvider MockDescriptorCacheDataProvider { get; set; }
        protected ICacheProvider CacheProvider { get; set; }
        protected IEdFiOdsInstanceIdentificationProvider MockEdFiOdsInstanceIdentificationProvider { get; set; }
        protected IConfigValueProvider MockConfigValueProvider { get; set; }

        private ITypesAndDescriptorsCache MockNHibernateCallsAndInitializeCache()
        {
            MockTypeCacheDataProvider = MockRepository.GenerateStub<ITypeLookupProvider>();
            MockTypeCacheDataProvider.Stub(mtcdp => mtcdp.GetAllTypeLookups())
                .Return(new Dictionary<string, IList<TypeLookup>> { { TestTypeName, new List<TypeLookup> { TestType } } });
            MockDescriptorCacheDataProvider = MockRepository.GenerateStub<IDescriptorLookupProvider>();
            MockDescriptorCacheDataProvider.Stub(mdcdp => mdcdp.GetAllDescriptorLookups())
                .Return(new Dictionary<string, IList<DescriptorLookup>> { { TestDescriptorName, new List<DescriptorLookup> { TestDescriptorNormal, TestDescriptorCustom } } });
            CacheProvider = new MemoryCacheProvider();
            MockEdFiOdsInstanceIdentificationProvider =
                MockRepository.GenerateStub<IEdFiOdsInstanceIdentificationProvider>();
            MockEdFiOdsInstanceIdentificationProvider.Stub(mefoiip => mefoiip.GetInstanceIdentification()).Return(1);

            MockConfigValueProvider = MockRepository.GenerateStub<IConfigValueProvider>();
            MockConfigValueProvider.Stub(mcvp => mcvp.GetValue(Arg<string>.Is.Equal("DescriptorNamespacePrefix")))
                .Return(DefaultNamespacePrefix);
            return new TypesAndDescriptorsCache(MockTypeCacheDataProvider, MockDescriptorCacheDataProvider, CacheProvider, MockEdFiOdsInstanceIdentificationProvider, MockConfigValueProvider);
        }

        [Test]
        public void GetId_ForType_ByValue_FindsCorrectId()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            var returnedId = cache.GetId(TestTypeName, "Attendance Award");
            Assert.AreEqual(TestType.Id, returnedId);
        }

        [Test]
        public void GetId_ForDescriptor_ByFullyQualifiedValue_FindsCorrectId()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            var returnedId = cache.GetId(TestDescriptorName, "http://www.ed-fi.org/Descriptor/AcademicSubjectDescriptor.xml/110");
            Assert.AreEqual(TestDescriptorNormal.Id, returnedId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetId_ForDescriptor_ByFullyQualifiedValue_IncorrectFormatting_DoesNotFindCorrectId()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            cache.GetId(TestDescriptorName, "http://www.ed-fi.org110");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetId_ForDescriptor_ByFullyQualifiedValue_IncorrectFormatting2_DoesNotFindCorrectId()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            cache.GetId(TestDescriptorName, "www.ed-fi.org/110");
        }

        [Test]
        public void GetId_ForDescriptor_ByUnQualifiedValue_FindsCorrectId_IfExistsUnderDefaultNamespace()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            var returnedId = cache.GetId(TestDescriptorName, "110");
            Assert.AreEqual(TestDescriptorNormal.Id, returnedId);
        }

        [Test]
        public void GetId_ForDescriptor_ForMissingValue_Forces_DatabaseFetch_And_Cache_Update()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            MockDescriptorCacheDataProvider.Stub(mdcdp => mdcdp.GetDescriptorLookupsByDescriptorName(TestDescriptorName))
                .Return(new List<DescriptorLookup>
                {
                    TestDescriptorNormal,
                    TestDescriptorCustom,
                    TestDescriptorCustomNotIncluded
                });
            SystemClock.Now = () => DateTime.UtcNow.AddHours(1);
            var returnedId = cache.GetId(TestDescriptorName, "http://www.changetest.org/Descriptor/AcademicSubjectDescriptor.xml/210");
            MockDescriptorCacheDataProvider.AssertWasCalled(mdcdp => mdcdp.GetDescriptorLookupsByDescriptorName(TestDescriptorName));
            Assert.AreEqual(TestDescriptorCustomNotIncluded.Id, returnedId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetId_ForDescriptor_ForMissingValue_Throws_Exception_If_DatabaseSynchronization_Also_Did_Not_Find_It()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            SystemClock.Now = () => DateTime.UtcNow.AddHours(1);
            cache.GetId(TestDescriptorName, "BLAHBLAHBLAH");
        }


        [Test]
        public void GetValue_ForType_ReturnsValue()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            var returnedValue = cache.GetValue(TestTypeName, TestType.Id);
            Assert.AreEqual(TestType.ShortDescription, returnedValue);
        }

        [Test]
        public void GetValue_ForDescriptor_CustomNamespaceValue_ReturnsFullyQualifiedValue()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            SystemClock.Now = () => DateTime.UtcNow.AddHours(1);
            var returnedValue = cache.GetValue(TestDescriptorName, TestDescriptorCustom.Id);
            var fullyQualifiedValueForTestDescriptorCustom = TestDescriptorCustom.Namespace + "/" + TestDescriptorCustom.CodeValue;
            Assert.AreEqual(fullyQualifiedValueForTestDescriptorCustom, returnedValue);
        }

        [Test]
        public void GetValue_ForDescriptor_DefaultDomainValue_ReturnsNonFullyQualifiedValue()
        {
            var cache = MockNHibernateCallsAndInitializeCache();
            var returnedValue = cache.GetValue(TestDescriptorName, TestDescriptorNormal.Id);
            Assert.AreEqual(TestDescriptorNormal.CodeValue, returnedValue);
        }
    }
}
