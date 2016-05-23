using System;
using System.Linq;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.NHibernate;
using EdFi.Ods.Entities.NHibernate.GradeLevelTypeAggregate;
using EdFi.Ods.Entities.NHibernate.SchoolYearTypeAggregate;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Repositories.NHibernate.Tests
{
    public class TypeLookupProviderTests : BaseDatabaseTest
    {
        private static readonly string _databaseName = string.Format("TypeLookupProviderTests_{0}",
            Guid.NewGuid().ToString("N"));

        protected override string DatabaseName
        {
            get { return _databaseName; }
        }

        protected override string BaseDatabase
        {
            get { return "EdFi_Ods_Populated_Template"; }
        }

        private const string GradeLevelTypeName = "GradeLevelType";
        private const string SchoolYearTypeName = "SchoolYearType";
        private GradeLevelType GradeLevelTestType1 { get; set; }
        private GradeLevelType GradeLevelTestType2 { get; set; }
        private SchoolYearType SchoolYearTestType { get; set; }
        private ITypeLookupProvider TypeLookupProvider { get; set; }

        [SetUp]
        public void SetUp()
        {
            GradeLevelTestType1 = new GradeLevelType
            {
                CodeValue = "Test Grade Level 1",
                ShortDescription = "Test Grade Level 1",
                Description = "Test Grade Level 1"
            };

            GradeLevelTestType2 = new GradeLevelType
            {
                CodeValue = "Test Grade Level 2",
                ShortDescription = "Test Grade Level 2",
                Description = "Test Grade Level 2"
            };

            SchoolYearTestType = new SchoolYearType
            {
                CurrentSchoolYear = false,
                SchoolYear = 9999,
                SchoolYearDescription = "Test School Year"
            };

            using (var session = SessionFactory.OpenSession())
            {
                session.Save(GradeLevelTestType1);
                session.Save(GradeLevelTestType2);
                session.Save(SchoolYearTestType);
            }

            TypeLookupProvider = new TypeLookupProvider(SessionFactory);
        }

        [Test]
        public void When_getting_valid_type_by_id_should_return_lookup_data()
        {
            var testLookup = TypeLookupProvider.GetSingleTypeLookupById(GradeLevelTypeName,
                GradeLevelTestType1.GradeLevelTypeId);
            testLookup.ShouldNotBeNull();
            testLookup.Id.ShouldEqual(GradeLevelTestType1.GradeLevelTypeId);
            testLookup.ShortDescription.ShouldEqual(GradeLevelTestType1.ShortDescription);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void When_getting_invalid_type_name_by_id_should_throw_argument_exception()
        {
            TypeLookupProvider.GetSingleTypeLookupById("Invalid",
                GradeLevelTestType1.GradeLevelTypeId);
        }

        [Test]
        public void When_getting_invalid_type_by_id_should_return_null()
        {
            var testLookup = TypeLookupProvider.GetSingleTypeLookupById(GradeLevelTypeName, 999999999);
            testLookup.ShouldBeNull();
        }

        [Test]
        public void When_getting_valid_type_by_short_description_should_return_lookup_data()
        {
            var testLookup = TypeLookupProvider.GetSingleTypeLookupByShortDescription(GradeLevelTypeName,
                GradeLevelTestType1.ShortDescription);
            testLookup.ShouldNotBeNull();
            testLookup.Id.ShouldEqual(GradeLevelTestType1.GradeLevelTypeId);
            testLookup.ShortDescription.ShouldEqual(GradeLevelTestType1.ShortDescription);
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void When_getting_invalid_type_name_by_short_description_should_throw_argument_exception()
        {
            TypeLookupProvider.GetSingleTypeLookupByShortDescription("Invalid",
                GradeLevelTestType1.ShortDescription);
        }

        [Test]
        public void When_getting_invalid_type_by_short_description_should_return_null()
        {
            var testLookup = TypeLookupProvider.GetSingleTypeLookupByShortDescription(GradeLevelTypeName,
                "Invalid Grade Level Type");
            testLookup.ShouldBeNull();
        }

        [Test]
        public void When_getting_all_types_should_return_both_entries()
        {
            var typesLookup = TypeLookupProvider.GetAllTypeLookups();
            var testLookup1 =
                typesLookup.Values.SelectMany(tlv => tlv)
                    .SingleOrDefault(tl => tl.TypeName == GradeLevelTypeName && tl.Id == GradeLevelTestType1.GradeLevelTypeId);
            testLookup1.ShouldNotBeNull();
            testLookup1.Id.ShouldEqual(GradeLevelTestType1.GradeLevelTypeId);
            testLookup1.ShortDescription.ShouldEqual(GradeLevelTestType1.ShortDescription);
            var testLookup2 =
                typesLookup.Values.SelectMany(tlv => tlv)
                    .SingleOrDefault(tl => tl.TypeName == GradeLevelTypeName && tl.Id == GradeLevelTestType2.GradeLevelTypeId);
            testLookup2.ShouldNotBeNull();
            testLookup2.Id.ShouldEqual(GradeLevelTestType2.GradeLevelTypeId);
            testLookup2.ShortDescription.ShouldEqual(GradeLevelTestType2.ShortDescription);
        }

        [Test]
        public void When_getting_all_types_should_not_return_any_school_year_type()
        {
            var typesLookup = TypeLookupProvider.GetAllTypeLookups();
            var testLookup = typesLookup.Values.SelectMany(tlv => tlv).FirstOrDefault(tl => tl.TypeName == SchoolYearTypeName);
            testLookup.ShouldBeNull();
        }
    }
}
