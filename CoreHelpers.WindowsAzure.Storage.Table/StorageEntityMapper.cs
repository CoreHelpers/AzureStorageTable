using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	public class StorageEntityMapper
	{
		public String PartitionKeyFormat { get; set; }
		public String RowKeyFormat { get; set; }
        public nVirtualValueEncoding RowKeyEncoding { get; set; }
        public String TableName { get; set; }

        public StorageEntityMapper() 
        {}

        public StorageEntityMapper(StorageEntityMapper src) 
        {
            this.PartitionKeyFormat = src.PartitionKeyFormat;
            this.RowKeyFormat = src.RowKeyFormat;
            this.RowKeyEncoding = src.RowKeyEncoding;
            this.TableName = src.TableName;
        }
	}
}
