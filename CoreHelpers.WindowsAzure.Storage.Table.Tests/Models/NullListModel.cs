using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
    [Storable]
    public class NullListModel
    {

        [PartitionKey]
        public string P { get; set; } = "P1";

        [RowKey]
        public string R { get; set; } = "R1";

        [StoreAsJsonObject(typeof(List<string>))]
        public List<string> Items { get; set; }
    }
}

