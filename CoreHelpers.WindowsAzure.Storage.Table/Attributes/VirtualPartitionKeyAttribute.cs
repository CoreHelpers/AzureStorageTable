using System;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{	
	[AttributeUsage(AttributeTargets.Class)]
	public class VirtualPartitionKeyAttribute : Attribute
	{
		public string PartitionKeyFormat { get; set; }
		
		public VirtualPartitionKeyAttribute(string PartitionKeyFormat) {
			this.PartitionKeyFormat = PartitionKeyFormat;
		}
	}
}
