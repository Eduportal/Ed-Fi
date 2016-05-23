using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Common.Context;
using EdFi.Common.Database;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Data.Repositories;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.Common.UniqueIdIntegration;
using EdFi.Ods.Entities.Common._Installers;
using EdFi.Ods.Entities.NHibernate;
using EdFi.Ods.Entities.NHibernate.ProgramAggregate;
using EdFi.Ods.Entities.NHibernate.StudentAggregate;
using EdFi.Ods.Entities.NHibernate.StudentProgramAssociationAggregate;
using EdFi.Ods.Entities.NHibernate.StudentTitleIPartAProgramAssociationAggregate;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture.Bytecode;
using EdFi.Ods.Entities.Repositories.NHibernate._Installers;
using EdFi.Ods.Security.Authorization.Repositories;
using EdFi.Ods.Security.Claims;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests.EdFi.Ods.Security._Stubs;
using NCrunch.Framework;
using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework;
using Should;
using Test.Common;
using ServiceDescriptor = EdFi.Ods.Entities.NHibernate.ServiceDescriptorAggregate.ServiceDescriptor;
using Environment = NHibernate.Cfg.Environment;

namespace EdFi.Ods.Repositories.NHibernate.Tests
{
    [TestFixture]
    public class StudentProgramAssociationTests
    {

        [TestFixture]
        public class When_upserting_student_program_and_derived_assocations : TestFixtureBase
        {
            private string StudentGuid;
            private string StudentUniqueId;

            private int educationOrganization1;
            private int educationOrganization2;

            private ServiceDescriptor serviceDescriptor;
            private Program program1;
            private Program program2;
            private Student student;

            // Dependencies
            private IWindsorContainer container;
            private IUpsertEntity<Student> studentRepo;
            private IUpsertEntity<Program> programRepo;
            private IUpsertEntity<StudentProgramAssociation> studentProgramAssociationRepo;
            private IUpsertEntity<StudentTitleIPartAProgramAssociation> studentTitleIPartAProgramAssociationRepo;

            private IGetEntitiesBySpecification<ServiceDescriptor> getServicesBySpecification;
            private IDeleteEntityById<ServiceDescriptor> deleteServiceById;
            private IUpsertEntity<ServiceDescriptor> upsertService;

            // Supplied values

            // Actual values
            private UpsertResults actualStudentTitleIPartAUpsert1Results;
            private UpsertResults actualStudentTitleIPartAUpsert2Results;
            private UpsertResults actualStudentProgramAssociationUpsert1Results;
            private UpsertResults actualStudentProgramAssociationUpsert2Results;
            private Exception actualDeleteStudentProgramAssociationException;
            private Exception actualDeleteStudentTitleIPartAAssociationException;
            private bool actualServiceDeletedWithSecondUpsert;
            private bool actualServiceDeletedWithSecondDerivedClassUpsert;
            private bool actualProgramAssociationDeleted;
            private bool actualTitleIProgramAssociationDeleted;

            protected override void EstablishContext()
            {
                try
                {
                    RegisterDependencies();
                    ClaimsPrincipal.ClaimsPrincipalSelector =
                        () => EdFiClaimsPrincipalSelector.GetClaimsPrincipal(container.Resolve<IClaimsIdentityProvider>());

                    studentRepo = container.Resolve<IUpsertEntity<Student>>();
                    programRepo = container.Resolve<IUpsertEntity<Program>>();
                    studentProgramAssociationRepo = container.Resolve<IUpsertEntity<StudentProgramAssociation>>();
                    studentTitleIPartAProgramAssociationRepo =
                        container.Resolve<IUpsertEntity<StudentTitleIPartAProgramAssociation>>();

                    getServicesBySpecification =
                            new GetEntitiesBySpecification<ServiceDescriptor>(
                                container.Resolve<ISessionFactory>(),
                                container.Resolve<IGetEntitiesByIds<ServiceDescriptor>>());
                    deleteServiceById = container.Resolve<IDeleteEntityById<ServiceDescriptor>>();
                    upsertService = container.Resolve<IUpsertEntity<ServiceDescriptor>>();

                    InitializeEducationOrganizationIdsForTest();

                    StudentGuid = Guid.NewGuid().ToString();
                    StudentUniqueId = Guid.NewGuid().ToString("N");

                    serviceDescriptor = GetTestServiceDescriptor();
                    program1 = GetProgram1();
                    program2 = GetProgram2();
                    student = GetStudent();

                    ReinitializeTestData();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            protected override void ExecuteBehavior()
            {
                bool isCreated, isModified;

                // ----------------------------------------------------------------------
                // Create a new StudentProgramAssociation with a service
                // ----------------------------------------------------------------------
                var studentProgramAssociation = new StudentProgramAssociation
                {
                    StudentUniqueId = student.StudentUniqueId,
                    ProgramType = program1.ProgramType,
                    ProgramName = program1.ProgramName,
                    ProgramEducationOrganizationId = educationOrganization1,
                    BeginDate = DateTime.Today.AddDays(-60),
                    EducationOrganizationId = educationOrganization1,
                };

                var service = new StudentProgramAssociationService
                {
                    ServiceDescriptor = string.Format("{0}{1}", serviceDescriptor.Namespace, serviceDescriptor.CodeValue),
                    PrimaryIndicator = true,
                };

                studentProgramAssociation.StudentProgramAssociationServices.Add(service);

                studentProgramAssociationRepo.Upsert(studentProgramAssociation, false, out isModified, out isCreated);
                actualStudentProgramAssociationUpsert1Results = new UpsertResults
                {
                    IsCreated = isCreated,
                    IsModified = isModified
                };


                // ----------------------------------------------------------------------
                // Update the StudentProgramAssociation, removing the service
                // ----------------------------------------------------------------------
                var studentProgramAssociation2 = new StudentProgramAssociation
                {
                    StudentUniqueId = student.StudentUniqueId,
                    ProgramType = program1.ProgramType,
                    ProgramName = program1.ProgramName,
                    ProgramEducationOrganizationId = educationOrganization1,
                    BeginDate = DateTime.Today.AddDays(-60),
                    ReasonExitedDescriptor = "Moved out of state",
                    EducationOrganizationId = educationOrganization1,
                };

                studentProgramAssociationRepo.Upsert(studentProgramAssociation2, false, out isModified, out isCreated);

                actualStudentProgramAssociationUpsert2Results = new UpsertResults
                {
                    IsCreated = isCreated,
                    IsModified = isModified
                };

                // Verify the service got removed
                using (var conn = GetSqlConnectionForOds())
                {
                    conn.Open();
                    var cmd = new SqlCommand(string.Format("SELECT COUNT(*) FROM edfi.StudentProgramAssociationService WHERE ProgramName = '{0}'", program1.ProgramName), conn);
                    actualServiceDeletedWithSecondUpsert = 0 == Convert.ToInt32(cmd.ExecuteScalar());
                }

                // -------------------------------------------------------------------------------------------------------------
                // Create a new (derived class) StudentTitleIPartAProgramAssociation with a service hanging off the base class
                // -------------------------------------------------------------------------------------------------------------
                var studentTitleIPartA = new StudentTitleIPartAProgramAssociation
                {
                    // PK
                    StudentUniqueId = student.StudentUniqueId,
                    ProgramType = program2.ProgramType,
                    ProgramName = program2.ProgramName,
                    ProgramEducationOrganizationId = educationOrganization1,
                    BeginDate = DateTime.Today.AddDays(-60),
                    EducationOrganizationId = educationOrganization2,

                    // Base class property
                    ReasonExitedDescriptor = "Moved out of state",

                    // Derived class property
                    TitleIPartAParticipantType = "Local Neglected Program",
                };

                // Add a service to the base class
                var titleIService = new StudentProgramAssociationServiceForBase
                {
                    ServiceDescriptor = string.Format("{0}{1}", serviceDescriptor.Namespace, serviceDescriptor.CodeValue),
                    PrimaryIndicator = true,
                };

                studentTitleIPartA.StudentProgramAssociationServices.Add(titleIService);

                studentTitleIPartAProgramAssociationRepo.Upsert(studentTitleIPartA, false, out isModified, out isCreated);

                actualStudentTitleIPartAUpsert1Results = new UpsertResults
                {
                    IsCreated = isCreated,
                    IsModified = isModified
                };


                // -------------------------------------------------------------------------------------------------------------
                // Create a new (derived class) StudentTitleIPartAProgramAssociation with a service hanging off the base class
                // -------------------------------------------------------------------------------------------------------------
                var studentTitleIPartA2 = new StudentTitleIPartAProgramAssociation
                {
                    // PK
                    StudentUniqueId = student.StudentUniqueId,
                    ProgramType = program2.ProgramType,
                    ProgramName = program2.ProgramName,
                    ProgramEducationOrganizationId = educationOrganization1,
                    BeginDate = DateTime.Today.AddDays(-60),
                    EducationOrganizationId = educationOrganization2,

                    // Base class property
                    ReasonExitedDescriptor = "Graduated with a high school diploma",

                    // Derived class property
                    TitleIPartAParticipantType = "Public Schoolwide Program",
                };

                // Updating derived record, removing the service
                studentTitleIPartAProgramAssociationRepo.Upsert(studentTitleIPartA2, false, out isModified, out isCreated);

                actualStudentTitleIPartAUpsert2Results = new UpsertResults
                {
                    IsCreated = isCreated,
                    IsModified = isModified
                };

                // Verify the service got removed
                using (var conn = GetSqlConnectionForOds())
                {
                    conn.Open();
                    var cmd = new SqlCommand(string.Format("SELECT COUNT(*) FROM edfi.StudentProgramAssociationService WHERE ProgramName = '{0}'", program2.ProgramName), conn);
                    actualServiceDeletedWithSecondDerivedClassUpsert = 0 == Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Clean up the data now
                // Delete the concrete base class (StudentProgramAssociation)
                try
                {
                    var deleteStudentProgramAssociationByKey = container.Resolve<IDeleteEntityByKey<StudentProgramAssociation>>();

                    deleteStudentProgramAssociationByKey.DeleteByKey(studentProgramAssociation, null);
                }
                catch (Exception ex)
                {
                    actualDeleteStudentProgramAssociationException = ex;
                }

                // Verify the program association got removed
                using (var conn = GetSqlConnectionForOds())
                {
                    conn.Open();
                    var cmd = new SqlCommand(string.Format("SELECT COUNT(*) FROM edfi.StudentProgramAssociationService WHERE ProgramName = '{0}'", program1.ProgramName), conn);
                    actualProgramAssociationDeleted = 0 == Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Delete the derived class (StudentProgramAssociation)
                try
                {
                    var deletestudentTitleIPartAAssociationById =
                        container.Resolve<IDeleteEntityById<StudentTitleIPartAProgramAssociation>>();

                    deletestudentTitleIPartAAssociationById.DeleteById(studentTitleIPartA.Id, null);
                }
                catch (Exception ex)
                {
                    actualDeleteStudentTitleIPartAAssociationException = ex;
                }

                // Verify the TitleI program association got removed
                using (var conn = GetSqlConnectionForOds())
                {
                    conn.Open();
                    var cmd = new SqlCommand(string.Format("SELECT COUNT(*) FROM edfi.StudentProgramAssociationService WHERE ProgramName = '{0}'", program2.ProgramName), conn);
                    actualTitleIProgramAssociationDeleted = 0 == Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            private SqlConnection GetSqlConnectionForOds()
            {
                var regKey = typeof(IDatabaseConnectionStringProvider).GetServiceNameWithSuffix(Databases.Ods.ToString());

                var databaseConnectionStringProvider = container.Resolve<IDatabaseConnectionStringProvider>(regKey);

                return new SqlConnection(databaseConnectionStringProvider.GetConnectionString());
            }

            protected override void AfterBehaviorExecution()
            {
                CleanupSupportingTestData();
            }

            [Test]
            [ExclusivelyUses(TestSingletons.EduIdDatabase)]
            public void Should_create_concrete_base_program_association_with_first_upsert()
            {
                actualStudentProgramAssociationUpsert1Results.IsCreated.ShouldBeTrue();
            }

            [Test]
            [ExclusivelyUses(TestSingletons.EduIdDatabase)]
            public void Should_update_concrete_base_program_association_with_second_upsert()
            {
                actualStudentProgramAssociationUpsert2Results.IsModified.ShouldBeTrue();
            }

            [Test]
            [ExclusivelyUses(TestSingletons.EduIdDatabase)]
            public void Should_remove_associated_program_service_with_second_upsert()
            {
                actualServiceDeletedWithSecondUpsert.ShouldBeTrue();
            }

            [Test]
            [ExclusivelyUses(TestSingletons.EduIdDatabase)]
            public void Should_delete_concrete_base_program_association_without_an_exception()
            {
                actualDeleteStudentProgramAssociationException.ShouldBeNull();
                actualProgramAssociationDeleted.ShouldBeTrue();
            }

            [Test]
            [ExclusivelyUses(TestSingletons.EduIdDatabase)]
            public void Should_create_derived_student_TitleIPartA_assocation_with_first_upsert()
            {
                actualStudentTitleIPartAUpsert1Results.IsCreated.ShouldBeTrue();
            }

            [Test]
            [ExclusivelyUses(TestSingletons.EduIdDatabase)]
            public void Should_update_derived_student_TitleIPartA_association_with_second_upsert()
            {
                actualStudentTitleIPartAUpsert2Results.IsModified.ShouldBeTrue();
            }

            [Test]
            [ExclusivelyUses(TestSingletons.EduIdDatabase)]
            public void Should_remove_associated_program_service_from_TitleIPartA_association_with_second_upsert()
            {
                actualServiceDeletedWithSecondDerivedClassUpsert.ShouldBeTrue();
            }

            [Test]
            [ExclusivelyUses(TestSingletons.EduIdDatabase)]
            public void Should_delete_derived_student_TitleIPartA_association_without_an_exception()
            {
                actualDeleteStudentTitleIPartAAssociationException.ShouldBeNull();
                actualTitleIProgramAssociationDeleted.ShouldBeTrue();
            }

            #region Support methods

            private ServiceDescriptor GetTestServiceDescriptor()
            {
                return new ServiceDescriptor
                {
                    CodeValue = string.Format("{0}_Test", DateTime.Now.Ticks),
                    Description = string.Format("{0}_Test", DateTime.Now.Ticks),
                    ShortDescription = string.Format("{0}_Test", DateTime.Now.Ticks),
                    Namespace = "http://TEST/",
                };
            }

            private Program GetProgram1()
            {
                return new Program
                {
                    EducationOrganizationId = educationOrganization1,
                    ProgramType = "Athletics",
                    ProgramName = string.Format("{0}_1", DateTime.Now.Ticks)
                };
            }

            private Program GetProgram2()
            {
                return new Program
                {
                    EducationOrganizationId = educationOrganization1,
                    ProgramType = "College Preparatory",
                    ProgramName = string.Format("{0}_2", DateTime.Now.Ticks)
                };
            }

            private Student GetStudent()
            {
                return new Student
                {
                    FirstName = "Bob",
                    LastSurname = "Jones",
                    SexType = "Male",
                    BirthDate = DateTime.Today.AddYears(-25),
                    HispanicLatinoEthnicity = false,
                    StudentUniqueId = StudentUniqueId,
                    Id = new Guid(StudentGuid)
                };
            }

            private void RegisterDependencies()
            {
                var factory = new InversionOfControlContainerFactory();
                container = factory.CreateContainer(c =>
                {
                    c.AddFacility<TypedFactoryFacility>();
                    c.AddFacility<DatabaseConnectionStringProviderFacility>();
                });
                container.Install(new EdFiOdsCommonInstaller());
                container.Install(new DatabaseConnectionStringProvidersInstaller());
                container.Install(new EdFiOdsRepositoriesNHibernateInstaller());
                container.Install(new EdFiOdsEntitiesCommonInstaller());
                container.Register(Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());

                container.Register(Component.For<ICacheProvider>().ImplementedBy<MemoryCacheProvider>());
                InitializeNHibernate(container);

                // Register security component
                container.Register(Component
                    .For<IAuthorizationContextProvider>()
                    .ImplementedBy<AuthorizationContextProvider>());

                container.Register(Component
                    .For<IContextStorage>()
                    .ImplementedBy<HashtableContextStorage>());

                container.Register(
                    Component.For<IClaimsIdentityProvider>()
                        .ImplementedBy<ClaimsIdentityProvider>()
                        .LifestyleTransient());

                container.Register(
                    Component.For<ISecurityRepository>()
                    .ImplementedBy<StubSecurityRepository>());

            }

            private class UpsertResults
            {
                public bool IsModified { get; set; }
                public bool IsCreated { get; set; }
            }

            protected void ReinitializeTestData()
            {
                bool isCreated, isModified;

                CleanupSupportingTestData();

                CreateTestServiceDescriptor();

                //TypesAndDescriptorsCache.UseInternalInstance();
                //PersonIdentifiersCache.UseInternalInstance();

                studentRepo.Upsert(student, false, out isModified, out isCreated);

                programRepo.Upsert(program1, false, out isModified, out isCreated);

                programRepo.Upsert(program2, false, out isModified, out isCreated);
            }

            private void InitializeEducationOrganizationIdsForTest()
            {
                // Verify the service got removed
                using (var conn = GetSqlConnectionForOds())
                {
                    conn.Open();
                    var cmd = new SqlCommand("SELECT EducationOrganizationId FROM edfi.EducationOrganization", conn);
                    using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        try
                        {
                            reader.Read();
                            educationOrganization1 = reader.GetInt32(0);
                            reader.Read();
                            educationOrganization2 = reader.GetInt32(0);

                            reader.Dispose();
                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Unable to load two Education Organization Ids from the configured ODS database.",
                                ex);
                        }
                    }
                }
            }

            private void CleanupSupportingTestData()
            {
                using (var db = new UniqueIdIntegrationContext())
                {
                    var mapping = db.UniqueIdPersonMappings.SingleOrDefault(m => m.UniqueId == StudentUniqueId);
                    if (mapping != null)
                    {
                        db.UniqueIdPersonMappings.Remove(mapping);
                        db.SaveChanges();
                    }

                    db.UniqueIdPersonMappings.Add(new UniqueIdPersonMapping { Id = Guid.Parse(StudentGuid), UniqueId = StudentUniqueId });
                    db.SaveChanges();
                }

                using (var conn = GetSqlConnectionForOds())
                {
                    conn.Open();

                    string sqlScript = string.Format(@"
                        -- Delete program associations
                        delete from edfi.StudentProgramAssociationService where ProgramName IN ('{0}', '{1}');
                        delete from edfi.StudentTitleIPartAProgramAssociation where ProgramName IN ('{0}', '{1}');
                        delete from edfi.StudentProgramAssociation where ProgramName IN ('{0}', '{1}');

                        -- Delete Student
                        delete from edfi.Student where StudentUniqueId = '{2}';

                        -- Delete Programs
                        delete from edfi.Program where ProgramName = '{0}';
                        delete from edfi.Program where ProgramName = '{1}';",

                        program1.ProgramName, program2.ProgramName, StudentUniqueId);

                    var cmd = new SqlCommand(sqlScript, conn);
                    cmd.ExecuteNonQuery();
                }

                DeleteTestServiceDescriptor();
            }

            private void CreateTestServiceDescriptor()
            {
                bool isModified;
                bool isCreated;

                upsertService.Upsert(serviceDescriptor, false, out isModified, out isCreated);
            }

            private void DeleteTestServiceDescriptor()
            {
                var services = getServicesBySpecification.GetBySpecification(new ServiceDescriptor(), QueryParameters.Empty);
                var serviceToDelete = services.SingleOrDefault(x => x.CodeValue == serviceDescriptor.CodeValue);

                if (serviceToDelete != null)
                    deleteServiceById.DeleteById(serviceToDelete.Id, null);
            }

            private static void InitializeNHibernate(IWindsorContainer container)
            {
                Environment.BytecodeProvider = new BytecodeProvider(container);
                var nHibernateConfiguration = new Configuration().Configure();

                nHibernateConfiguration.AddCreateDateHooks();

                container
                    .Register(Component
                        .For<ISessionFactory>()
                        .UsingFactoryMethod(nHibernateConfiguration.BuildSessionFactory)
                        .LifeStyle.Singleton);

                container.Register(Component
                    .For<EdFiOdsConnectionProvider>()
                    .DependsOn(Dependency
                        .OnComponent(
                            typeof(IDatabaseConnectionStringProvider),
                            typeof(IDatabaseConnectionStringProvider).GetServiceNameWithSuffix(Databases.Ods.ToString()))));
            }

            #endregion
        }
    }
}
