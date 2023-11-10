using System;
using Azure.Data.Tables;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using System.Linq;
using CoreHelpers.WindowsAzure.Storage.Table.Internal;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public partial class StorageContext : IStorageContext
    {
        public async Task ExportToJsonAsync(string tableName, TextWriter writer, Action<ImportExportOperation> onOperation)
        {
            try
            {
                var tc = GetTableClient(GetTableName(tableName));

                var existsTable = await tc.ExistsAsync();
                if (!existsTable)
                    throw new FileNotFoundException($"Table '{tableName}' does not exist");

                // build the json writer
                JsonWriter wr = new JsonTextWriter(writer);
                
                // prepare the array in result
                wr.WriteStartArray();

                // enumerate all items from a table
                var tablePages = tc.QueryAsync<TableEntity>().AsPages();

                // do the backup
                await foreach (var page in tablePages)
                {
                    if (onOperation != null)
                        onOperation(ImportExportOperation.processingPage);

                    foreach (var entity in page.Values)
                    {
                        if (onOperation != null)
                            onOperation(ImportExportOperation.processingItem);

                        wr.WriteStartObject();
                        wr.WritePropertyName(TableConstants.RowKey);
                        wr.WriteValue(entity.RowKey);
                        wr.WritePropertyName(TableConstants.PartitionKey);
                        wr.WriteValue(entity.PartitionKey);
                        wr.WritePropertyName(TableConstants.Properties);
                        wr.WriteStartArray();
                        foreach (var propertyKvp in entity)
                        {
                            if (propertyKvp.Key.Equals(TableConstants.PartitionKey) || propertyKvp.Key.Equals(TableConstants.RowKey) || propertyKvp.Key.Equals("odata.etag") || propertyKvp.Key.Equals(TableConstants.Timestamp))
                                continue;

                            wr.WriteStartObject();
                            wr.WritePropertyName(TableConstants.PropertyName);
                            wr.WriteValue(propertyKvp.Key);
                            wr.WritePropertyName(TableConstants.PropertyType);
                            wr.WriteValue(propertyKvp.Value.GetType().GetEdmPropertyType());
                            wr.WritePropertyName(TableConstants.PropertyValue);

                            switch (propertyKvp.Value.GetType().GetEdmPropertyType())
                            {
                                case ExportEdmType.DateTime:
                                    wr.WriteValue(((DateTime)propertyKvp.Value).ToUniversalTime());
                                    break;
                                default:
                                    wr.WriteValue(propertyKvp.Value);
                                    break;
                            }

                            wr.WriteEndObject();
                        }
                        wr.WriteEnd();
                        wr.WriteEndObject();
                    }

                    if (onOperation != null)
                        onOperation(ImportExportOperation.processedPage);
                }

                // finishe the export
                wr.WriteEnd();
                wr.Flush();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ImportFromJsonAsync(string tableName, StreamReader reader, Action<ImportExportOperation> onOperation)
        {
            // get the tableclient
            var tc = GetTableClient(GetTableName(tableName));

            // ensure table exists
            if (!await tc.ExistsAsync())
                await tc.CreateAsync();            

            // store the entities by partition key
            var entityStore = new Dictionary<string, List<TableEntity>>();

            // parse
            JsonSerializer serializer = new JsonSerializer();
            using (var jsonReader = new JsonTextReader(reader))
            {
                while (jsonReader.Read())
                {
                    // deserialize only when there's "{" character in the stream
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        // get the data model 
                        var currentModel = serializer.Deserialize<ImportExportTableEntity>(jsonReader);

                        foreach (var property in currentModel.Properties)
                        {
                            if ((ExportEdmType)property.PropertyType == ExportEdmType.String && property.PropertyValue is DateTime)
                            {
                                property.PropertyValue = ((DateTime)property.PropertyValue).ToString("o");
                            } else if ((ExportEdmType)property.PropertyType == ExportEdmType.DateTime) {
                                property.PropertyValue = ((DateTime)property.PropertyValue).ToUniversalTime();
                            }
                        }

                        // convert to table entity
                        var tableEntity = GetTableEntity(currentModel);

                        // add to the right store
                        if (!entityStore.ContainsKey(tableEntity.PartitionKey))
                            entityStore.Add(tableEntity.PartitionKey, new List<TableEntity>());

                        // add the entity 
                        entityStore[tableEntity.PartitionKey].Add(tableEntity);

                        // check if we need to offload this table 
                        if (entityStore[tableEntity.PartitionKey].Count == 100)
                        {
                            // insert the partition
                            await tc.SubmitTransactionAsync(entityStore[tableEntity.PartitionKey].Select(e => new TableTransactionAction(TableTransactionActionType.UpsertReplace, e)));
                            
                            // clear
                            entityStore.Remove(tableEntity.PartitionKey);
                        }
                    }
                }

                // post processing
                foreach (var kvp in entityStore)
                {
                    // insert the partition
                    await tc.SubmitTransactionAsync(kvp.Value.Select(e => new TableTransactionAction(TableTransactionActionType.UpsertReplace, e)));                    
                }
            }
        }

        private TableEntity GetTableEntity(ImportExportTableEntity data)
        {
            var teBuilder = new TableEntityBuilder();

            teBuilder.AddPartitionKey(data.PartitionKey);
            teBuilder.AddRowKey(data.RowKey);
            
            foreach (var prop in data.Properties)
            {
                teBuilder.AddProperty(prop.PropertyName, prop.PropertyValue);                
            }

            return teBuilder.Build();
        }
    }
}