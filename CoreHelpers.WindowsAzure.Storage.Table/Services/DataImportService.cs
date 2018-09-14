using CoreHelpers.WindowsAzure.Storage.Table.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Services
{
    internal class DataImportService : DataService
    {
        public const string TableName = "CoreHelpersTableImportLogs";

        public DataImportService(StorageContext storageContext) 
            : base(storageContext)
        {
        }

        public async Task ImportFromJsonStreamAsync(string tableName, StreamReader streamReader, Action<int> progress = null) {

            // ensure table exists
            var targetTable = storageContext.GetTableReference(tableName);
            if (!await targetTable.ExistsAsync())
                await CreateAzureTableAsync(targetTable);
                
            // store the entities by partition key
            var entityStore = new Dictionary<string, List<DynamicTableEntity>>();

            // parse
            JsonSerializer serializer = new JsonSerializer();
            using (JsonReader reader = new JsonTextReader(streamReader))
            {
                while (reader.Read())
                {
                    // deserialize only when there's "{" character in the stream
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        // get the data model 
                        var currentModel = serializer.Deserialize<ImportExportTableEntity>(reader);

                        foreach(var property in currentModel.Properties) {
                            if ((EdmType)property.PropertyType == EdmType.String && property.PropertyValue is DateTime) {
                                property.PropertyValue = ((DateTime)property.PropertyValue).ToString("o");
                            }
                        }
                        // convert to table entity
                        var tableEntity = GetTableEntity(currentModel);

                        // add to the right store
                        if (!entityStore.ContainsKey(tableEntity.PartitionKey))
                            entityStore.Add(tableEntity.PartitionKey, new List<DynamicTableEntity>());

                        // add the entity 
                        entityStore[tableEntity.PartitionKey].Add(tableEntity);

                        // check if we need to offload this table 
                        if (entityStore[tableEntity.PartitionKey].Count == 100) {

                            // restoring
                            await RestorePageAsync(targetTable, entityStore[tableEntity.PartitionKey], progress);

                            // clear
                            entityStore.Remove(tableEntity.PartitionKey);
                        }
                    }
                }

                // post processing
                foreach(var kvp in entityStore) {

                    // restoring
                    await RestorePageAsync(targetTable, kvp.Value, progress);
                }
            }
        }

        private DynamicTableEntity GetTableEntity(ImportExportTableEntity data)
        {
            var azureEntity = new DynamicTableEntity();

            azureEntity.PartitionKey = data.PartitionKey;
            azureEntity.RowKey = data.RowKey;

            foreach (var prop in data.Properties)
            {
                var propertyValueType = (EdmType)prop.PropertyType;
                var propertyValue = GenerateProperty(propertyValueType, prop.PropertyValue);
                azureEntity.Properties.Add(new KeyValuePair<string, EntityProperty>(prop.PropertyName, propertyValue));
            }
            return azureEntity;
        }

        private async Task RestorePageAsync(CloudTable tableReference, IEnumerable<DynamicTableEntity> models, Action<int> progress)
        {
            // check that the list is small enough 
            if (models.Count() > TableConstants.TableServiceBatchMaximumOperations)
                throw new Exception("Entity Page is to big");

            // Create the batch
            var currentBatch = new TableBatchOperation();

            // add models
            foreach (var entity in models)
                currentBatch.InsertOrReplace(entity);

            // notify
            progress?.Invoke(currentBatch.Count);

            // insert
            await tableReference.ExecuteBatchAsync(currentBatch);
        }

        private async Task CreateAzureTableAsync(CloudTable table)
        {
            try
            {
                await table.CreateAsync();
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 409)
            {
                await Task.Delay(20000);
                await CreateAzureTableAsync(table);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}