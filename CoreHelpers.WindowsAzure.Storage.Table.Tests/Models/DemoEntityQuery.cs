using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
    [Storable(Tablename: "DemoEntityQuery")]
    public class DemoEntityQuery
    {
        [PartitionKey] public string P { get; set; } = "P1";

        [RowKey] public string R { get; set; } = "R1";

        public string StringField { get; set; } = String.Empty;

        public bool BoolField { get; set; }
    }
}

