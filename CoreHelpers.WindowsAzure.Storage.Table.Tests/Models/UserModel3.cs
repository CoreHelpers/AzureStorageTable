using System;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
 	[Storable()]
	public class UserModel3
	{
		[PartitionKey]
		public string P { get; set; } = "Partition01";
		
        [RowKey]
        public string Contact { get; set; } = String.Empty;

        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;

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
		public string CodeValue { get; set; } = String.Empty;
        public string CodeType { get; set; } = String.Empty;
    }
}
