using System;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	public class DynamicTableEntityMapper
	{
		public String PartitionKeyPropery { get; set; }
		public String RowKeyProperty { get; set; }
		public String TableName { get; set; }
	}
}
