using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.Models
{
	[Storable(Tablename = "HugeDemoEntry")]
	public class HugeDemoEntry
	{
		[PartitionKey]
		public string P { get; set; } = "Partition01";

		[RowKey]
		public string R { get; set; } = Guid.NewGuid().ToString();    		
	}
}
