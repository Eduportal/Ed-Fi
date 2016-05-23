using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Infrastructure;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class CsvResourceDataProfileProviderFixtures
    {
        public class When_loading_a_properly_formatted_CSV_file : TestFixtureBase
        {
            // Supplied values
            private string suppliedCsvContent = @"Table,Effective Ratio,Fixed Count
SomethingNotUsed,,0
SomethingStudentRelated,0.5,
#SomethingToBeIgnored,,0

SomethingFixedCount,,5
";

            // Actual values
            private IEnumerable<ResourceDataProfile> actualProfile;
            private Exception actualException;

            // External dependencies
            private IFile file;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                file = Stub<IFile>();
                file.Expect(f => f.ReadAllText("xyz.csv"))
                    .Return(suppliedCsvContent);
            }

            protected override void Act()
            {
                try
                {
                    // Perform the action to be tested
                    var provider = new CsvResourceDataProfileProvider(file);
                    actualProfile = provider.GetResourceDataProfiles("xyz.csv").ToList();
                }
                catch (Exception ex)
                {
                    actualException = ex;
                }
            }

            [Assert]
            public void Should_not_throw_an_exception()
            {
                actualException.ShouldBeNull();
            }

            [Assert]
            public void Should_ignore_header_line_and_only_process_the_data_while_ignoring_blank_or_commented_out_lines()
            {
                // Assert the expected results
                actualProfile.Count().ShouldEqual(3);
            }

            [Assert]
            public void Should_process_the_fixed_count_entry_for_the_unused_resource()
            {
                var unusedResource = actualProfile.SingleOrDefault(x => x.ResourceName == "SomethingNotUsed");
                unusedResource.ShouldNotBeNull();

                unusedResource.PerStudentRatio.ShouldBeNull();
                unusedResource.FixedCount.ShouldEqual(0);
            }

            [Assert]
            public void Should_process_the_entry_for_the_resource_based_on_a_per_student_ratio()
            {
                var studentRatioBasedResource = actualProfile.SingleOrDefault(x => x.ResourceName == "SomethingStudentRelated");
                studentRatioBasedResource.ShouldNotBeNull();

                studentRatioBasedResource.PerStudentRatio.ShouldEqual((decimal?) .5d);
                studentRatioBasedResource.FixedCount.ShouldBeNull();
            }

            [Assert]
            public void Should_process_the_fixed_count_entry_for_the_used_resource()
            {
                var usedResource = actualProfile.SingleOrDefault(x => x.ResourceName == "SomethingFixedCount");
                usedResource.ShouldNotBeNull();

                usedResource.PerStudentRatio.ShouldBeNull();
                usedResource.FixedCount.ShouldEqual(5);
            }


            [Assert]
            public void Should_not_process_the_entry_with_the_comment_marker_at_the_start_of_the_line()
            {
                actualProfile.Any(x => x
                    .ResourceName == "#SomethingToBeIgnored"
                    || x.ResourceName == "SomethingToBeIgnored")
                    .ShouldBeFalse();
            }
        }
    }
}
