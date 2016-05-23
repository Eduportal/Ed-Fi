using System;
using System.Linq;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.NHibernate.AssessmentPeriodDescriptorAggregate;
using EdFi.Ods.Entities.NHibernate.CountryDescriptorAggregate;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Repositories.NHibernate.Tests
{
    public class DescriptorLookupProviderTests : BaseDatabaseTest
    {
        private static readonly string _databaseName = string.Format("DescriptorLookupProviderTests_{0}",
            Guid.NewGuid().ToString("N"));

        protected override string DatabaseName
        {
            get { return _databaseName; }
        }

        protected override string BaseDatabase
        {
            get { return "EdFi_Ods_Populated_Template"; }
        }

        private const string CountryDescriptorName = "CountryDescriptor";
        private CountryDescriptor CountryTestDescriptor1 { get; set; }
        private CountryDescriptor CountryTestDescriptor2 { get; set; }
        private CountryDescriptor CountryTestDescriptor3 { get; set; }
        private const string AssessmentPeriodDescriptorName = "AssessmentPeriodDescriptor";
        private AssessmentPeriodDescriptor AssessmentPeriodTestDescriptor1 { get; set; }
        private IDescriptorLookupProvider DescriptorLookupProvider { get; set; }

        [SetUp]
        public void SetUp()
        {
            CountryTestDescriptor1 = new CountryDescriptor
            {
                Namespace = "http://namespace1/",
                CodeValue = "Test Country Descriptor 1",
                ShortDescription = "Test Country Descriptor 1",
                Description = "Test Country Descriptor 1",
                EffectiveBeginDate = DateTime.Now
            };

            CountryTestDescriptor2 = new CountryDescriptor
            {
                Namespace = "http://namespace1/",
                CodeValue = "Test Country Descriptor 2",
                ShortDescription = "Test Country Descriptor 2",
                Description = "Test Country Descriptor 2",
                EffectiveBeginDate = DateTime.Now
            };

            CountryTestDescriptor3 = new CountryDescriptor
            {
                Namespace = "http://namespace2/",
                CodeValue = "Test Country Descriptor 1",
                ShortDescription = "Test Country Descriptor 1",
                Description = "Test Country Descriptor 1",
                EffectiveBeginDate = DateTime.Now
            };

            AssessmentPeriodTestDescriptor1 = new AssessmentPeriodDescriptor
            {
                Namespace = "http://namespace1/",
                CodeValue = "Test Assessment Period Descriptor 1",
                ShortDescription = "Test Assessment Period Descriptor 1",
                Description = "Test Assessment Period Descriptor 1",
                EffectiveBeginDate = DateTime.Now
            };

            using (var session = SessionFactory.OpenSession())
            {
                session.Save(CountryTestDescriptor1);
                session.Save(CountryTestDescriptor2);
                session.Save(CountryTestDescriptor3);
                session.Save(AssessmentPeriodTestDescriptor1);
            }

            DescriptorLookupProvider = new DescriptorLookupProvider(SessionFactory);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void When_getting_invalid_descriptor_name_by_id_should_throw_argument_exception()
        {
            DescriptorLookupProvider.GetSingleDescriptorLookupById("Invalid",
                CountryTestDescriptor1.CountryDescriptorId);
        }

        [Test]
        public void When_getting_valid_descriptor_by_id_should_return_lookup_data()
        {
            var testLookup = DescriptorLookupProvider.GetSingleDescriptorLookupById(CountryDescriptorName,
                CountryTestDescriptor1.CountryDescriptorId);
            testLookup.ShouldNotBeNull();
            testLookup.Id.ShouldEqual(CountryTestDescriptor1.CountryDescriptorId);
            testLookup.CodeValue.ShouldEqual(CountryTestDescriptor1.CodeValue);
            testLookup.Namespace.ShouldEqual(CountryTestDescriptor1.Namespace);
        }

        [Test]
        public void When_getting_invalid_descriptor_by_id_should_return_null()
        {
            var testLookup = DescriptorLookupProvider.GetSingleDescriptorLookupById(CountryDescriptorName, 999999999);
            testLookup.ShouldBeNull();
        }

        [Test]
        public void When_getting_all_descriptor_lookups_by_descriptor_name_should_only_return_descriptors_for_that_name()
        {
            var descriptorsLookup = DescriptorLookupProvider.GetDescriptorLookupsByDescriptorName(AssessmentPeriodDescriptorName);
            descriptorsLookup.All(dl => dl.DescriptorName == AssessmentPeriodDescriptorName).ShouldBeTrue();
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void When_getting_all_descriptor_lookups_by_invalid_descriptor_name_should_throw_argument_exception()
        {
            DescriptorLookupProvider.GetDescriptorLookupsByDescriptorName("Invalid");
        }

        [Test]
        public void When_getting_all_descriptor_lookups_should_return_all_entries()
        {
            var descriptorsLookup = DescriptorLookupProvider.GetAllDescriptorLookups();
            var testLookup1 =
                descriptorsLookup.Values.SelectMany(tlv => tlv)
                    .SingleOrDefault(dl => dl.DescriptorName == CountryDescriptorName && dl.Id == CountryTestDescriptor1.CountryDescriptorId);
            testLookup1.ShouldNotBeNull();
            testLookup1.Id.ShouldEqual(CountryTestDescriptor1.CountryDescriptorId);
            testLookup1.CodeValue.ShouldEqual(CountryTestDescriptor1.CodeValue);
            testLookup1.Namespace.ShouldEqual(CountryTestDescriptor1.Namespace);
            var testLookup2 =
                descriptorsLookup.Values.SelectMany(tlv => tlv)
                    .SingleOrDefault(dl => dl.DescriptorName == CountryDescriptorName && dl.Id == CountryTestDescriptor2.CountryDescriptorId);
            testLookup2.ShouldNotBeNull();
            testLookup2.Id.ShouldEqual(CountryTestDescriptor2.CountryDescriptorId);
            testLookup2.CodeValue.ShouldEqual(CountryTestDescriptor2.CodeValue);
            testLookup2.Namespace.ShouldEqual(CountryTestDescriptor2.Namespace);
            var testLookup3 =
                descriptorsLookup.Values.SelectMany(tlv => tlv)
                    .SingleOrDefault(
                        dl =>
                            dl.DescriptorName == CountryDescriptorName &&
                            dl.Id == CountryTestDescriptor3.CountryDescriptorId);
            testLookup3.ShouldNotBeNull();
            testLookup3.Id.ShouldEqual(CountryTestDescriptor3.CountryDescriptorId);
            testLookup3.CodeValue.ShouldEqual(CountryTestDescriptor3.CodeValue);
            testLookup3.Namespace.ShouldEqual(CountryTestDescriptor3.Namespace);
            var testLookup4 =
                descriptorsLookup.Values.SelectMany(tlv => tlv)
                    .SingleOrDefault(
                        dl =>
                            dl.DescriptorName == AssessmentPeriodDescriptorName &&
                            dl.Id == AssessmentPeriodTestDescriptor1.AssessmentPeriodDescriptorId);
            testLookup4.ShouldNotBeNull();
            testLookup4.Id.ShouldEqual(AssessmentPeriodTestDescriptor1.AssessmentPeriodDescriptorId);
            testLookup4.CodeValue.ShouldEqual(AssessmentPeriodTestDescriptor1.CodeValue);
            testLookup4.Namespace.ShouldEqual(AssessmentPeriodTestDescriptor1.Namespace);
        }
    }
}