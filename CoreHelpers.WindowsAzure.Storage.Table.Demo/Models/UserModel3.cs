using System;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.Models
{
 	[Storable()]
	public class UserModel3
	{
		[PartitionKey]
		public string P { get; set; } = "Partition01";
		
        [RowKey]
        public string Contact { get; set; }
    
		public string FirstName { get; set; } 
		public string LastName { get; set; }
		
		[StoreAsJsonObject(typeof(List<Code>))]
		public List<ICode> Codes { get; set; } = new List<ICode>();
	}
	
	public interface ICode 
	{
		string CodeValue { get; set; }
		string CodeType { get; set; }
	}

	public class Code : ICode
	{
		public string CodeValue { get; set; }
		public string CodeType { get; set; }
	}
}
