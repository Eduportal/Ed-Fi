namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using global::EdFi.Ods.Api.Models.Resources.EducationServiceCenter;
    using global::EdFi.Ods.Api.Models.Resources.LocalEducationAgency;
    using global::EdFi.Ods.Api.Models.Resources.School;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    public class When_shredding_test_xml_file : TestFixtureBase
    {
        private string filePath;
        private readonly IList<object> entities = new List<object>(); 
        protected override void EstablishContext()
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", string.Empty);
            filePath = Path.Combine(rootPath, @"EdFi.Ods.XmlShredding\Xml\Interchange-EducationOrganization.xml");
        }
        
        protected override void ExecuteTest()
        {
            var educationOrganizationFactoryHelper = new EducationOrganizationFactoryHelper(new AddressFactory(),
                                                   new EducationOrganizationIdentificationCodeFactory(),
                                                   new EducationOrganizationCategoryFactory(),
                                                   new EducationOrganizationInstitutionInstitutionTelephoneFactory());
            var parser = new InterchangeXmlParser(
                    new IOldResourceFactory[]
                        {
                            new CourseFactory(new DescriptorReferenceTypeResolver()), 
                            new ClassPeriodFactory(), 
                            new LocationFactory(), 
                            new SchoolFactory(educationOrganizationFactoryHelper), 
                            new StateEducationAgencyFactory(educationOrganizationFactoryHelper), 
                            new AddressFactory(), 
                            new EducationServiceCenterFactory(educationOrganizationFactoryHelper),
                            new LocalEducationAgencyFactory(educationOrganizationFactoryHelper), 
                            new ProgramFactory()
                        },
                    new InterchangeFileManager(new InterchangeFileIndex())
                );

            foreach (var entity in parser.Load(filePath))
            {
                entities.Add(entity);
            }

        }

        [Test]
        public void Should_find_one_education_service_center()
        {
            entities.OfType<EducationServiceCenter>().Count().ShouldEqual(1);
        }

        [Test]
        public void Should_find_one_lea()
        {
            entities.OfType<LocalEducationAgency>().Count().ShouldEqual(1);
        }

        [Test]
        public void Should_find_five_schools()
        {
            entities.OfType<School>().Count().ShouldEqual(5);
        }
    }
}
