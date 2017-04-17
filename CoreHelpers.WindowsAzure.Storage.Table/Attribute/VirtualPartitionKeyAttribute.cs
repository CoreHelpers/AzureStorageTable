using System;
namespace CoreHelpers.WindowsAzure.Storage.Table
{	
	public class VirtualPartitionKeyAttribute : Attribute
	{
		public string PartitionKeyFormat { get; set; }
		
		public VirtualPartitionKeyAttribute(string PartitionKeyFormat) {
			this.PartitionKeyFormat = PartitionKeyFormat;
		}
	}
}
