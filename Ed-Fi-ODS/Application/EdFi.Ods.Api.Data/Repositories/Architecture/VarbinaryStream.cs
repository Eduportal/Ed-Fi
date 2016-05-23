﻿using System;
using System.IO;
using System.Linq;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;

namespace EdFi.Ods.Api.Data.Repositories.Architecture
{
    public class VarbinaryStream : Stream
    {
        private readonly string _keyValue;
        private readonly IVarbinaryWriter _varbinaryWriter;

        private int _offset;

        public VarbinaryStream(string keyValue, IVarbinaryWriter varbinaryWriter)
        {
            _keyValue = keyValue;
            _varbinaryWriter = varbinaryWriter;
        }

        public override void Write(byte[] buffer, int index, int count)
        {
            _varbinaryWriter.Write(_keyValue, buffer.Skip(index).Take(count).ToArray(), _offset, count);
            _offset = count;
        }

        #region unimplemented methods

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        #endregion unimplemented methods
    }
}