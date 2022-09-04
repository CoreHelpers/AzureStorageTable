using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHelpers.WindowsAzure.Storage.Table.Internal
{
    internal class ImportExportTableEntity
    {
        public ImportExportTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Properties = new List<ImportExportTablePropertyEntity>();
        }
        public List<ImportExportTablePropertyEntity> Properties { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}
