using System;
namespace CoreHelpers.WindowsAzure.Storage.Table
{
	[AttributeUsage(AttributeTargets.Class)]
	public class VirtualPartitonKeyAttribute : Attribute
	{
		public string PartitionKeyFormat { get; set; }
		
		public VirtualPartitonKeyAttribute(string PartitionKeyFormat) {
			this.PartitionKeyFormat = PartitionKeyFormat;
		}
	}
}
