using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo
{		
    [Storable()]
	public class UserModel2
	{
		[PartitionKey]
		public string P { get; set; } = "Partition01";
		
        [RowKey]
        public string Contact { get; set; }
    
		public string FirstName { get; set; } 
		public string LastName { get; set; }                		
	}
}
