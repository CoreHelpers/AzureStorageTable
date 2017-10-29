using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.Models
{
    [Storable(Tablename = "DemoEntryWithOptinalValues")]
    public class DemoEntryWithOptionalValues
    {
        [PartitionKey]
        public string PartitionKey { get; set; } = "P1";

        [RowKey]
        public string Identifier { get; set; }

        public string Name { get; set; }
        public double? Costs { get; set; }
    }
}
