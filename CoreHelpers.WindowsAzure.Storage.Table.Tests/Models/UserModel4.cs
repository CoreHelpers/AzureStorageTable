using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{

    public enum UserTypeEnum
    {
        Free,
        Pro
    }

    [Storable()]
    public class UserModel4
    {
        [PartitionKey]
        public string P { get; set; } = "Partition01";

        [RowKey]
        public string Contact { get; set; } = String.Empty;

        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;

        [StoreEnumAsString]
        public UserTypeEnum UserType { get; set; }

    }
}