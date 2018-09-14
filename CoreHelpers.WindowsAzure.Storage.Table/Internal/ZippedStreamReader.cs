using System;
using System.IO;
using System.IO.Compression;

namespace CoreHelpers.WindowsAzure.Storage.Table.Internal
{
    public class ZippedStreamReader : StreamReader
    {
        private bool compressed { get; set; }

        public ZippedStreamReader(Stream stream, bool compressed)
            : base(compressed ? new GZipStream(stream, CompressionMode.Decompress) : stream)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (compressed)
                this.BaseStream.Dispose();
        }
    }
}
