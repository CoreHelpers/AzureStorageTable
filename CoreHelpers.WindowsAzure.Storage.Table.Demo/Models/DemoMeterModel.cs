using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using System.Collections.Generic;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.Models
{
    [Storable(Tablename: "DemoMeterModel")]
    public class DemoMeterModel
    {
        [PartitionKey]
        public string P { get; set; } = "P1";

        [RowKey]
        public string R { get; set; } = "R1";

        [VirtualList(PropertyFormat: "DC{{index}}", Digits: 2)]
        public List<Double> ExtendedCosts { get; set; }
    }
}
