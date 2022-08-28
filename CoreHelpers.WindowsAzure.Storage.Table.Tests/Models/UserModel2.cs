using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{		
    [Storable()]
	public class UserModel2
	{
		[PartitionKey]
		public string P { get; set; } = "Partition01";
		
        [RowKey]
        public string Contact { get; set; } = String.Empty;

        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
    }
}
