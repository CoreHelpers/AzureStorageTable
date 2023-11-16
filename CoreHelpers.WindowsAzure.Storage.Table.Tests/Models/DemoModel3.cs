using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
    [Storable]
    public class DemoModel3
    {

        [PartitionKey]
        public string P { get; set; } = "P1";

        [RowKey]
        public string R { get; set; } = "R1";

        public string UserContact { get; set; } = "em@acme.org";

        [RelatedTable("Partition01", RowKey = "UserContact")]
        public UserModel2? User { get; set; }
    }
}

