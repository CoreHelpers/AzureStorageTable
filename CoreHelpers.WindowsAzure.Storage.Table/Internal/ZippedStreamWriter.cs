using System;
using System.IO;
using System.IO.Compression;

namespace CoreHelpers.WindowsAzure.Storage.Table.Internal
{
    public class ZippedStreamWriter : StreamWriter
    {    
        private bool compress { get; set; }

        public ZippedStreamWriter(Stream stream, bool compress)            
            : base(compress ? new GZipStream(stream, CompressionMode.Compress) : stream)
        {
            this.compress = compress;
        }

        protected override void Dispose(bool disposing)
        {
            if (compress)
                this.BaseStream.Dispose();
        }
    }
}
