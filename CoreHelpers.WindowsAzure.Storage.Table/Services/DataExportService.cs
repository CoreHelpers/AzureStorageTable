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
                var tableData = new List<DynamicTableEntity>();
                // await LogInformationAsync($"Started exporting table '{tableName}'");
                var table = storageContext.GetTableReference(tableName);

                var existsTable = await table.ExistsAsync();
                if (!existsTable)
                {
                    var message = $"Table '{tableName}' does not exist";
                    // await LogErrorAsync(message);
                    throw new FileNotFoundException(message);
                }
                // await LogInformationAsync($"Found table '{tableName}'");

                var tableQuery = new TableQuery<DynamicTableEntity>();
                TableContinuationToken tableContinuationToken = null;
                var querySegmentIndex = 0;
                // await LogInformationAsync($"Starting to process '{tableName}' data");

                JsonWriter wr = new JsonTextWriter(writer);

                do
                {
                    //await LogInformationAsync($"Reading segment '{querySegmentIndex}' of '{tableName}'");
                    var queryResponse = await table.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken, null, null);
                    var queryResponseEntitiesCount = queryResponse.Results.Count;
                    // await LogInformationAsync($"Got segment '{querySegmentIndex}' of '{tableName}' with {queryResponseEntitiesCount} entities");
                    
                    tableData.AddRange(queryResponse.Results.ToList());

                    tableContinuationToken = queryResponse.ContinuationToken;
                    
                    // await LogInformationAsync($"Written segment '{querySegmentIndex}' of '{tableName}' to target writer");
                    ++querySegmentIndex;
                    progress?.Invoke(queryResponseEntitiesCount);
                }
                while (tableContinuationToken != null);

                var linesProcessed = 0;
                wr.WriteStartArray();
                foreach (var entity in tableData)
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
                    ++linesProcessed;
                }

                wr.WriteEnd();
                wr.Flush();

                // await LogInformationAsync($"Finished writing '{tableName}' with '{linesProcessed}' lines of data");
            }
            catch (Exception e)
            {
                // await LogErrorAsync($"Error when exporting '{tableName}': {e.ToString()}");
                throw e;
            }
        }
    }
}