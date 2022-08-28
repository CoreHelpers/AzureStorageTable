using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using System.Collections.Generic;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
    [Storable(Tablename: "DemoMeterModel")]
    public class DemoMeterModel
    {
        [PartitionKey]
        public string P { get; set; } = "P1";

        [RowKey]
        public string R { get; set; } = "R1";

        [VirtualList(PropertyFormat: "DC{{index}}", Digits: 2)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public List<Double> ExtendedCosts { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
