using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
    [Storable]
    public class DemoModel2
    {

        [PartitionKey]
        public string P { get; set; } = "P1";

        [RowKey]
        public string R { get; set; } = "R1";

        public DateTime CreatedAt { get; set; } = DateTime.MinValue;
    }
}

