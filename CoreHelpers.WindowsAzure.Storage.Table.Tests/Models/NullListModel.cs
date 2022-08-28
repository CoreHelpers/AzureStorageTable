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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public List<string> Items { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}

