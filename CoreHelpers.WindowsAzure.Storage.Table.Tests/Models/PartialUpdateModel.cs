using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
    [Storable(Tablename = "PartialUpdate")]
    public class PartialUpdateModel
    {

        [PartitionKey]
        public string CustomerId { get; set; } = String.Empty;

        [RowKey]
        public string MeterId { get; set; } = String.Empty;

        public int? Value01 { get; set; }
        public int? Value02 { get; set; }
        public int? Value03 { get; set; }        
    }
}

