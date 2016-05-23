using EdFi.Ods.Admin.Models;
using NCrunch.Framework;
using NUnit.Framework;
using Should;
using Test.Common;

namespace EdFi.Ods.Admin.Tests.Models
{
    public class UsersContextMigrationsTests
    {
        [TestFixture]
        [ExclusivelyUses(TestSingletons.EmptyAdminDatabase)]
        public class When_migrating_from_empty_database
        {
            [SetUp]
            public void Setup()
            {
                using (var context = new UsersContext())
                {
                    context.Database.Delete();
                    context.Database.Initialize(false);
                }
            }

            [Test]
            public void Should_create_new_compatible_database()
            {
                using (var context = new UsersContext())
                {
                    context.Database.CompatibleWithModel(true).ShouldBeTrue();
                }
            }
        }
    }
}