using System;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	public class DynamicTableEntityMapper
	{
		public String PartitionKeyFormat { get; set; }
		public String RowKeyFormat { get; set; }
		public String TableName { get; set; }

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
