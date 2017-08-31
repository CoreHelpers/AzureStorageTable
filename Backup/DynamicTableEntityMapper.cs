using System;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	public class DynamicTableEntityMapper
	{
		public String PartitionKeyFormat { get; set; }
		public String RowKeyFormat { get; set; }
		public String TableName { get; set; }
	}
}
