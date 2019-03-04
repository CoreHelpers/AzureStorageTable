using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Services
{
    internal class DataExportService : DataService
    {
        public const string TableName = "TableExportLogs";

        public DataExportService(StorageContext storageContext) : base(storageContext)
        {
        }

        public async Task ExportToJson(string tableName, TextWriter writer, Action<int> progress = null)
        {
            try
            {
                var table = storageContext.GetTableReference(tableName);

                var existsTable = await table.ExistsAsync();
                if (!existsTable)
                {
                    var message = $"Table '{tableName}' does not exist";
                    throw new FileNotFoundException(message);
                }

                var tableQuery = new TableQuery<DynamicTableEntity>();
                TableContinuationToken tableContinuationToken = null;
                var querySegmentIndex = 0;

                JsonWriter wr = new JsonTextWriter(writer);

                // prepare the array in result
                wr.WriteStartArray();

                // download the pages
                do
                {
                    // query the data
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken, null, null);
                    var queryResponseEntitiesCount = queryResponse.Results.Count;

                    // move to next segment
                    tableContinuationToken = queryResponse.ContinuationToken;

                    // process the segment
                    ++querySegmentIndex;
                    progress?.Invoke(queryResponseEntitiesCount);

                    // do the backup
                    foreach (var entity in queryResponse.Results)
                    {
                        wr.WriteStartObject();
                        wr.WritePropertyName(TableConstants.RowKey);
                        wr.WriteValue(entity.RowKey);
                        wr.WritePropertyName(TableConstants.PartitionKey);
                        wr.WriteValue(entity.PartitionKey);
                        wr.WritePropertyName(TableConstants.Properties);
                        wr.WriteStartArray();
                        foreach (var propertyKvp in entity.Properties)
                        {
                            wr.WriteStartObject();
                            wr.WritePropertyName(TableConstants.PropertyName);
                            wr.WriteValue(propertyKvp.Key);
                            wr.WritePropertyName(TableConstants.PropertyType);
                            wr.WriteValue(propertyKvp.Value.PropertyType);
                            wr.WritePropertyName(TableConstants.PropertyValue);
                            wr.WriteValue(GetPropertyValue(propertyKvp.Value.PropertyType, propertyKvp.Value));
                            wr.WriteEndObject();
                        }
                        wr.WriteEnd();
                        wr.WriteEndObject();
                    }
                }
                while (tableContinuationToken != null);

                // finishe the export
                wr.WriteEnd();
                wr.Flush();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}