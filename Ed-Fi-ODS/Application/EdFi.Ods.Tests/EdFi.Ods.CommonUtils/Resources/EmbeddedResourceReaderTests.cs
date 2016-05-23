using System;
using System.IO;
using EdFi.Ods.Common.Utils.Resources;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils.Resources
{
    public class EmbeddedResourceReaderTests
    {
        [TestFixture]
        public class When_resource_exists_and_we_retrieve_the_string
        {
            [Test]
            public void Should_read_entire_resource_content()
            { 
                var result = EmbeddedResourceReader.GetResourceString<EmbeddedResourceReaderTests>("IAmAResource.txt");
                result.ShouldEqual("I have some content, I promise." + Environment.NewLine + "I am a second line.");
            }
        }

        [TestFixture]
        public class When_resource_exists_and_we_retrieve_the_stream
        {
            [Test]
            public void Should_read_entire_resource_content()
            {
                using(var stream = EmbeddedResourceReader.GetResourceStream<EmbeddedResourceReaderTests>("IAmAResource.txt"))
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    result.ShouldEqual("I have some content, I promise." + Environment.NewLine + "I am a second line.");
                }
            }
        }
    }
}