using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.TestObjects.Builders;
using NUnit.Framework;
using Should;

namespace EdFi.TestObjects.Tests.Builders
{
    public class DateTimeBuilderFixtures
    {
        [TestFixture]
        public class When_building_a_DateTime
        {
            private ValueBuildResult _actualResult;
            private ValueBuildResult _actualResult2;

            [TestFixtureSetUp]
            public void Act()
            {
                var builder = new DateTimeBuilder();

                _actualResult = builder.TryBuild(
                    new BuildContext("xxx.xxx", typeof(DateTime?), null, null, null, BuildMode.Create));
                _actualResult2 = builder.TryBuild(
                    new BuildContext("xxx.xxx", typeof(DateTime?), null, null, null, BuildMode.Create));
            }

            [Test]
            public void Should_generate_a_value()
            {
                _actualResult.Value.ShouldBeInRange(DateTime.Today.AddYears(-11), DateTime.Today.AddYears(-9));
                _actualResult2.Value.ShouldBeInRange(DateTime.Today.AddYears(-11), DateTime.Today.AddYears(-9));
            }
        }
    }
}
