// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using EdFi.Common.Extensions;
using EdFi.Ods.Tests._Bases;
using Should;

namespace EdFi.Ods.Tests.EdFi.Common.Extensions
{
    public class When_trimming_a_suffix : TestFixtureBase
    {
        [Assert]
        public void Should_trim_a_suffix_only_if_the_case_matches()
        {
            "aBcDeFgH".TrimSuffix("fGh").ShouldEqual("aBcDeFgH");
            "aBcDeFgH".TrimSuffix("FgH").ShouldEqual("aBcDe");
        }

        [Assert]
        public void Should_only_trim_a_suffix_it_appears_at_the_end_of_the_string()
        {
            "aBcDeFgH".TrimSuffix("eFg").ShouldEqual("aBcDeFgH");
        }

        [Assert]
        public void Should_trim_a_suffix_off_the_end_even_if_it_also_appears_in_the_middle_of_the_string()
        {
            "aBcD_gH_eFgH".TrimSuffix("gH").ShouldEqual("aBcD_gH_eF");
        }
    }
}