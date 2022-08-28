using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
    [Storable(Tablename = "DemoEntryWithOptinalValues")]
    public class DemoEntryWithOptionalValues
    {
        [PartitionKey]
        public string PartitionKey { get; set; } = "P1";

        [RowKey]
        public string Identifier { get; set; } = String.Empty;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public double? Costs { get; set; }
    }
}
