using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Data.Repositories.Architecture;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.Api.Services.Providers
{
    public class CustomMultipartStreamProvider : MultipartStreamProvider
    {
        private readonly string _fileChunkId;
        private readonly IVarbinaryWriter _varbinaryWriter;

        public CustomMultipartStreamProvider(string fileChunkId, IVarbinaryWriter varbinaryWriter)
        {
            _fileChunkId = fileChunkId;
            _varbinaryWriter = varbinaryWriter;
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            return new VarbinaryStream(_fileChunkId, _varbinaryWriter);
        }
    }
}