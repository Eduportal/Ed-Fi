using System;
using System.IO;
using EdFi.Ods.BulkLoad.Core;

namespace EdFi.Ods.Tests.TestObjects
{
    public class TestStreamBuilder : IStreamBuilder
    {
        private readonly Func<Stream> _streamFunc;

        public TestStreamBuilder(Func<Stream> streamFunc)
        {
            _streamFunc = streamFunc;
        }

        public Stream Build(string source)
        {
            return _streamFunc();
        }
    }
}