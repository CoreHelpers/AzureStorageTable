using System;
namespace CoreHelpers.WindowsAzure.Storage.Table.Demo
{
    [Storable()]
	public class UserModel2
	{                       
        [PartitionKey]
        [RowKey]
        public string Contact { get; set; }
    
		public string FirstName { get; set; } 
		public string LastName { get; set; }                		
	}
}
