using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.Models
{
    [Storable(Tablename: "DemoDatetimeModel")]
    public class DatetimeModel
    {
        [PartitionKey]
        public string P { get; set; } = "P1";

        [RowKey]
        public string R { get; set; } = "R1";

        public DateTime ActivatedAt { get; set; } = DateTime.MinValue;
    }
}
