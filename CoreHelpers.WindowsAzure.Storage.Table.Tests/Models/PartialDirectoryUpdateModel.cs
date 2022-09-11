using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{   
    [Storable(Tablename = "PartialDirectoryUpdate")]
    public class PartialDirectoryUpdateModel
    {

        [PartitionKey]
        public string CustomerId { get; set; } = String.Empty;

        [RowKey]
        public string MeterId { get; set; } = String.Empty;

        [VirtualDictionary("DC")]
        public Dictionary<int, double> Costs { get; set; } = new Dictionary<int, double>();
    }
}

