// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using NUnit.Framework;

namespace EdFi.TestObjects.Tests
{
    public class BuildContextTests
    {
        [TestFixture]
        public class When_creating_a_build_context_targeting_a_nullable_value_type
        {
            private BuildContext _actualBuildContext;

            [TestFixtureSetUp]
            public void Act()
            {
                _actualBuildContext = new BuildContext(null, typeof(int?), null, null, null, BuildMode.Create);
            }

            [Test]
            public void Should_report_a_target_type_of_int()
            {
                Assert.That(_actualBuildContext.TargetType, Is.SameAs(typeof(int)));
            }

            [Test]
            public void Should_report_a_raw_target_type_of_nullable_int()
            {
                Assert.That(_actualBuildContext.RawTargetType, Is.SameAs(typeof(int?)));
            }
        }
    }
}