using CoreHelpers.WindowsAzure.Storage.Table.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public async Task ImportFromJsonAsync(string tableName, string json, IStorageContextDelegate _delegate = null)
        {
            try
            {
                var entitiesToBeRetored = ParseTableEntities(json);
                await RestoreAsync(tableName, entitiesToBeRetored, storageContext, _delegate);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<DynamicTableEntity> ParseTableEntities(string json)
        {
            var tableModels = JsonConvert.DeserializeObject<List<ImportExportTableEntity>>(json, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
            var entities = tableModels.AsParallel().Select(GetTableEntity).ToList();
            return entities;
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

        public async Task RestoreAsync(string tableName, IEnumerable<DynamicTableEntity> models, StorageContext storageContext, IStorageContextDelegate _delegate)
        {
            // get a table reference
            var table = storageContext.GetTableReference(tableName);

            // verify if table exists
            var existsTable = await table.ExistsAsync();

            if (existsTable)
            {
                await table.DeleteIfExistsAsync();
                table = storageContext.GetTableReference(tableName);
            }
            await CreateAzureTableAsync(table);

            // Create the batch operation.
            List<TableBatchOperation> batchOperations = new List<TableBatchOperation>();

            // Create the first batch
            var currentBatch = new TableBatchOperation();
            batchOperations.Add(currentBatch);

            // define the modelcounter
            int modelCounter = 0;

            foreach (var entity in models)
            {
                currentBatch.Insert(entity);

                modelCounter++;

                if (modelCounter % TableConstants.TableServiceBatchMaximumOperations == 0)
                {
                    currentBatch = new TableBatchOperation();
                    batchOperations.Add(currentBatch);
                }
            }
            if (batchOperations.Any())
            {
                var tasks = batchOperations.Select(async bo =>
                {
                    await table.ExecuteBatchAsync(bo);
                    _delegate?.OnStored(typeof(DynamicTableEntity), nStoreOperation.insertOperation, bo.Count, null);
                });
                await Task.WhenAll(tasks);
            }
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