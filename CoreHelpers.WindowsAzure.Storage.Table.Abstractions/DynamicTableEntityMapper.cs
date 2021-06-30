using System;

namespace CoreHelpers.WindowsAzure.Storage.Table.Abstractions
{
	public class DynamicTableEntityMapper
	{
		public string PartitionKeyFormat { get; set; }
		public string RowKeyFormat { get; set; }
		public string TableName { get; set; }

        public DynamicTableEntityMapper() 
        {}

        public DynamicTableEntityMapper(DynamicTableEntityMapper src) 
        {
            this.PartitionKeyFormat = src.PartitionKeyFormat;
            this.RowKeyFormat = src.RowKeyFormat;
            this.TableName = src.TableName;
        }
	}
}
