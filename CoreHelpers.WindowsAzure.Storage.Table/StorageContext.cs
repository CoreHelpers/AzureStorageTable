﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	public class StorageContext : IDisposable
	{
		private enum nStoreOperation {
			insertOperation, 
			insertOrReplaceOperation,
			mergeOperation,
			mergeOrInserOperation
		}

		private CloudStorageAccount _storageAccount { get; set; }
		private Dictionary<Type, DynamicTableEntityMapper> _entityMapperRegistry { get; set; } = new Dictionary<Type, DynamicTableEntityMapper>();

		public StorageContext(string storageAccountName, string storageAccountKey)
		{
			_storageAccount 	= new CloudStorageAccount(new StorageCredentials(storageAccountName, storageAccountKey), true);
		}

		public void Dispose()
		{
			
		}

		public void AddEntityMapper(Type entityType, DynamicTableEntityMapper entityMapper)
		{
			_entityMapperRegistry.Add(entityType, entityMapper);
		}
        
        public void AddAttributeMapper() 
        {
            AddAttributeMapper(Assembly.GetEntryAssembly());

            /*foreach(var assembly in Assembly.GetEntryAssembly().GetReferencedAssemblies()) {
                AddAttributeMapper(assembly);
            } */          
        }

        internal void AddAttributeMapper(Assembly assembly)
        {
            var typesWithAttribute = assembly.GetTypesWithAttribute(typeof(StorableAttribute));
            foreach(var type in typesWithAttribute) {

                // get the concrete attribute
                var storableAttribute = type.GetTypeInfo().GetCustomAttribute<StorableAttribute>();                
                if (String.IsNullOrEmpty(storableAttribute.Tablename)) {
                    storableAttribute.Tablename = type.Name;
                }

                // store the neded properties
                PropertyInfo partitionKeyProperty = null;
                PropertyInfo rowKeyProperty = null;
                
                // get the partitionkey property & rowkey property
                var properties = type.GetRuntimeProperties();
                foreach (var property in properties)
                {
                    if (partitionKeyProperty != null && rowKeyProperty != null)
                        break;
                             
	                if (partitionKeyProperty == null && property.GetCustomAttribute<PartitionKeyAttribute>() != null)
	                    partitionKeyProperty = property;
	
	                if (rowKeyProperty == null && property.GetCustomAttribute<RowKeyAttribute>() != null)
	                    rowKeyProperty = property;		               
	            }

                // check 
                if (partitionKeyProperty == null || rowKeyProperty == null)
                    throw new Exception("Missing Partition or RowKey Attribute");
                    
                // build the mapper
                AddEntityMapper(type, new DynamicTableEntityMapper()
                {
                    TableName = storableAttribute.Tablename,
                    PartitionKeyPropery = partitionKeyProperty.Name,
                    RowKeyProperty = rowKeyProperty.Name
                });                      
            }
        }
                
		public Task CreateTableAsync<T>(bool ignoreErrorIfExists = true)
		{
			// Retrieve a reference to the table.
			CloudTable table = GetTableReference(GetTableName<T>());

			if (ignoreErrorIfExists)
			{
				// Create the table if it doesn't exist.
				return table.CreateIfNotExistsAsync();
			}
			else
			{
				// Create table and throw error
				return table.CreateAsync();
			}
		}

		public void CreateTable<T>(bool ignoreErrorIfExists = true)
		{
			this.CreateTableAsync<T>(ignoreErrorIfExists).GetAwaiter().GetResult();
		}

		public Task InsertAsync<T>(IEnumerable<T> models) where T : new ()
		{
			return this.StoreAsync(nStoreOperation.insertOperation, models);
		}

		public Task MergeAsync<T>(IEnumerable<T> models) where T : new()
		{
			return this.StoreAsync(nStoreOperation.mergeOperation, models);
		}

		public Task InsertOrReplaceAsync<T>(IEnumerable<T> models) where T : new()
		{
			return this.StoreAsync(nStoreOperation.insertOrReplaceOperation, models);
		}

		public Task MergeOrInsertAsync<T>(IEnumerable<T> models) where T : new()
		{
			return this.StoreAsync(nStoreOperation.mergeOrInserOperation, models);
		}

		public async Task<T> QueryAsync<T>(string partitionKey, string rowKey) where T : new()
		{
			var result = await QueryAsyncInternal<T>(partitionKey, rowKey, null);
			return result.FirstOrDefault<T>();
		}

		public async Task<IQueryable<T>> QueryAsync<T>(string partitionKey, TableContinuationToken continuationToken = null) where T : new()
		{
			return await QueryAsyncInternal<T>(partitionKey, null, continuationToken);
		}

		public async Task<IQueryable<T>> QueryAsync<T>(TableContinuationToken continuationToken = null) where T: new() 
		{
			return await QueryAsyncInternal<T>(null, null, continuationToken);
		}

		private string GetTableName<T>()
		{
			// lookup the entitymapper
			var entityMapper = _entityMapperRegistry[typeof(T)];

			// get the table name
			return entityMapper.TableName;
		}

		private Task StoreAsync<T>(nStoreOperation storaeOperationType, IEnumerable<T> models) where T : new()
		{
			// Retrieve a reference to the table.
			CloudTable table = GetTableReference(GetTableName<T>());

			// Create the batch operation.
			TableBatchOperation batchOperation = new TableBatchOperation();

			// lookup the entitymapper
			var entityMapper = _entityMapperRegistry[typeof(T)];

			// Add all items
			foreach (var model in models)
			{
				switch(storaeOperationType) {
					case nStoreOperation.insertOperation:
						batchOperation.Insert(new DynamicTableEntity<T>(model, entityMapper));
						break;
					case nStoreOperation.insertOrReplaceOperation:
						batchOperation.InsertOrReplace(new DynamicTableEntity<T>(model, entityMapper));
						break;
					case nStoreOperation.mergeOperation:
						batchOperation.Merge(new DynamicTableEntity<T>(model, entityMapper));
						break;
					case nStoreOperation.mergeOrInserOperation:
						batchOperation.InsertOrMerge(new DynamicTableEntity<T>(model, entityMapper));
						break;
				}
			}

			// execute 
			return table.ExecuteBatchAsync(batchOperation);
		}


		private async Task<IQueryable<T>> QueryAsyncInternal<T>(string partitionKey, string rowKey, TableContinuationToken continuationToken = null) where T : new()
		{
			// Retrieve a reference to the table.
			CloudTable table = GetTableReference(GetTableName<T>());

			// lookup the entitymapper
			var entityMapper = _entityMapperRegistry[typeof(T)];

			// Construct the query to get all entries
			TableQuery<DynamicTableEntity<T>> query = new TableQuery<DynamicTableEntity<T>>();

			// add partitionkey if exists
			if (partitionKey != null)
			 	query = query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

			// add row key if exists
			if (rowKey != null)
				query = query.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));

			// execute the query
			var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);

			// map all to the original models
			List<T> result = new List<T>();
			foreach (DynamicTableEntity<T> model in queryResult)
				result.Add(model.Model);

			// done 
			return result.AsQueryable();
		}

		private CloudTable GetTableReference(string tableName) {
			
			// create the table client 
			var storageTableClient = _storageAccount.CreateCloudTableClient();

			// Create the table client.
			CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

			// Retrieve a reference to the table.
			return tableClient.GetTableReference(tableName);
		}
	}
}
