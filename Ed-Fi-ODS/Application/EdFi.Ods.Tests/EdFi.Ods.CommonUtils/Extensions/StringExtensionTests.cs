namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils.Extensions
{
    using global::EdFi.Ods.Common.Utils.Extensions;

    using NUnit.Framework;

    using Should;

    public class StringExtensionTests
    {
        [TestFixture]
        public class When_converting_string_to_option_bool
        {
            [Test]
            public void Should_convert_false_to_false()
            {
                "fAlsE".AsOptionalBool().ShouldBeFalse();
            }

            [Test]
            public void Should_convert_true_to_true()
            {
                "true".AsOptionalBool().ShouldBeTrue();
            }

            [Test]
            public void Should_convert_null_to_false()
            {
                string input = null;
                input.AsOptionalBool().ShouldBeFalse();
            }

            [Test]
            public void Should_convert_unparseable_to_false()
            {
                "blarg".AsOptionalBool().ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_replacing_last_occurance
        {
            [Test]
            public void Should_not_replace_anything_if_zero_occurances()
            {
                "FooBarBaz".ReplaceLastOccurrence("Blah", "1").ShouldEqual("FooBarBaz");
            }

            [Test]
            public void Should_replace_a_single_occurance_at_the_start()
            {
                "FooBarBaz".ReplaceLastOccurrence("Foo", "1").ShouldEqual("1BarBaz");
            }

            [Test]
            public void Should_replace_last_occurance_when_that_occurance_is_not_the_ending()
            {
                "FooBarFooBaz".ReplaceLastOccurrence("Foo", "1").ShouldEqual("FooBar1Baz");
            }

            [Test]
            public void Should_replace_last_occurance_when_it_is_the_end()
            {
                "FooBarBazBar".ReplaceLastOccurrence("Bar", "1").ShouldEqual("FooBarBaz1");
            }
        }

        public class JoinWithCharacter
        {
            [TestFixture]
            public class When_base_uri_is_missing_trailing_slash_and_path_is_missing_leading_slash
            {
                private string _result;

                [TestFixtureSetUp]
                public void Setup()
                {
                    this._result = "http://www.example.com/somePath".JoinWithCharacter("someOtherPath/fooPath", '/');
                }

                [Test]
                public void Should_combine_paths_with_a_single_slash()
                {
                    this._result.ShouldEqual("http://www.example.com/somePath/someOtherPath/fooPath");
                }
            }

            [TestFixture]
            public class When_base_uri_is_missing_trailing_slash_and_path_has_leading_slash
            {
                private string _result;

                [TestFixtureSetUp]
                public void Setup()
                {
                    this._result = "http://www.example.com/somePath".JoinWithCharacter("/someOtherPath/fooPath", '/');
                }

                [Test]
                public void Should_combine_paths_with_a_single_slash()
                {
                    this._result.ShouldEqual("http://www.example.com/somePath/someOtherPath/fooPath");
                }
            }

            [TestFixture]
            public class When_base_uri_is_has_trailing_slash_and_path_is__leading_slash
            {
                private string _result;

                [TestFixtureSetUp]
                public void Setup()
                {
                    this._result = "http://www.example.com/somePath/".JoinWithCharacter("someOtherPath/fooPath", '/');
                }

                [Test]
                public void Should_combine_paths_with_a_single_slash()
                {
                    this._result.ShouldEqual("http://www.example.com/somePath/someOtherPath/fooPath");
                }
            }

            [TestFixture]
            public class When_base_uri_is_has_multiple_trailing_slash_and_path_has_multiple_leading_slash
            {
                private string _result;

                [TestFixtureSetUp]
                public void Setup()
                {
                    this._result = "http://www.example.com/somePath////".JoinWithCharacter("////someOtherPath/fooPath", '/');
                }

                [Test]
                public void Should_combine_paths_with_a_single_slash()
                {
                    this._result.ShouldEqual("http://www.example.com/somePath/someOtherPath/fooPath");
                }
            }
        }
    }
}