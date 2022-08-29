using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using System.IO;
using CoreHelpers.WindowsAzure.Storage.Table.Abstractions;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Services;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Azure.Data.Tables;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	public enum nStoreOperation {
		insertOperation, 
		insertOrReplaceOperation,
		mergeOperation,
		mergeOrInserOperation,
		delete
	}

	public class QueryResult<T>
	{
		public IQueryable<T> Items { get; internal set; }
		public TableContinuationToken NextToken { get; internal set; }
	}
	
	public class StorageContext : IStorageContext
	{		
		private CloudStorageAccount _storageAccount { get; set; }
		private Dictionary<Type, DynamicTableEntityMapper> _entityMapperRegistry { get; set; } = new Dictionary<Type, DynamicTableEntityMapper>();
		private bool _autoCreateTable { get; set; } = false;
		private IStorageContextDelegate _delegate { get; set; }
		private string _tableNamePrefix;
		private string _connectionString;

		public StorageContext(string storageAccountName, string storageAccountKey, string storageEndpointSuffix = null)
		{
            _connectionString = String.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2}", "https", storageAccountName, storageAccountKey);
			if (!String.IsNullOrEmpty(storageEndpointSuffix))
                _connectionString = String.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};EndpointSuffix={3}", "https", storageAccountName, storageAccountKey, storageEndpointSuffix);
			
			_storageAccount = CloudStorageAccount.Parse(_connectionString);
		}

        public StorageContext(string connectionString)
        {
			_connectionString = connectionString;
            _storageAccount = CloudStorageAccount.Parse(_connectionString);
        }

        public StorageContext(StorageContext parentContext)
		{
            // we reference the storage account
			_storageAccount = parentContext._storageAccount;
			
            // we reference the entity mapper
            _entityMapperRegistry = new Dictionary<Type, DynamicTableEntityMapper>(parentContext._entityMapperRegistry);

            // we are using the delegate
			this.SetDelegate(parentContext._delegate);

			// take the tablename prefix
			_tableNamePrefix = parentContext._tableNamePrefix;

			// store the connection string
			_connectionString = parentContext._connectionString;
        }

		public void Dispose()
		{
			
		}
		
		public void SetDelegate(IStorageContextDelegate delegateModel)
		{
			_delegate = delegateModel;		
		}

		public IStorageContext CreateChildContext()
		{
			return new StorageContext(this);
		}

		public IStorageContext EnableAutoCreateTable() 
		{
			_autoCreateTable = true;
			return this;
		}

		public void AddEntityMapper(Type entityType, DynamicTableEntityMapper entityMapper)
		{
			_entityMapperRegistry.Add(entityType, entityMapper);
		}

        public void RemoveEntityMapper(Type entityType)
        {
            if (_entityMapperRegistry.ContainsKey(entityType))
                _entityMapperRegistry.Remove(entityType);
        }
        
        public void AddAttributeMapper() 
        {
            AddAttributeMapper(Assembly.GetEntryAssembly());
			AddAttributeMapper(Assembly.GetCallingAssembly());			
		}

        internal void AddAttributeMapper(Assembly assembly)
        {
            var typesWithAttribute = assembly.GetTypesWithAttribute(typeof(StorableAttribute));
            foreach(var type in typesWithAttribute) {
                AddAttributeMapper(type);
            }
        }
        
        public void AddAttributeMapper(Type type) 
        {
        	AddAttributeMapper(type, string.Empty);
        }
        
        public void AddAttributeMapper(Type type, String optionalTablenameOverride ) 
        {
    		// get the concrete attribute
            var storableAttribute = type.GetTypeInfo().GetCustomAttribute<StorableAttribute>();                
            if (String.IsNullOrEmpty(storableAttribute.Tablename)) {
                storableAttribute.Tablename = type.Name;
            }

            // store the neded properties
            string partitionKeyFormat = null;
            string rowKeyFormat = null;
                                    
            // get the partitionkey property & rowkey property
            var properties = type.GetRuntimeProperties();
            foreach (var property in properties)
            {
                if (partitionKeyFormat != null && rowKeyFormat != null)
                    break;
                         
                if (partitionKeyFormat == null && property.GetCustomAttribute<PartitionKeyAttribute>() != null)
                    partitionKeyFormat = property.Name;
				
			 	if (rowKeyFormat == null && property.GetCustomAttribute<RowKeyAttribute>() != null)
					rowKeyFormat = property.Name;		               
            }

			// virutal partition key property
			var virtualPartitionKeyAttribute = type.GetTypeInfo().GetCustomAttribute<VirtualPartitionKeyAttribute>();
			if (virtualPartitionKeyAttribute != null && !String.IsNullOrEmpty(virtualPartitionKeyAttribute.PartitionKeyFormat))
				partitionKeyFormat = virtualPartitionKeyAttribute.PartitionKeyFormat;
			
			// virutal row key property
			var virtualRowKeyAttribute = type.GetTypeInfo().GetCustomAttribute<VirtualRowKeyAttribute>();
			if (virtualRowKeyAttribute != null && !String.IsNullOrEmpty(virtualRowKeyAttribute.RowKeyFormat))
				rowKeyFormat = virtualRowKeyAttribute.RowKeyFormat;
				
            // check 
            if (partitionKeyFormat == null || rowKeyFormat == null)
                throw new Exception("Missing Partition or RowKey Attribute");
                
            // build the mapper
            AddEntityMapper(type, new DynamicTableEntityMapper()
            {
                TableName = String.IsNullOrEmpty(optionalTablenameOverride) ? storableAttribute.Tablename : optionalTablenameOverride,
                PartitionKeyFormat = partitionKeyFormat,
                RowKeyFormat = rowKeyFormat
            });         
        } 
                       
        public IEnumerable<Type> GetRegisteredMappers() 
        {
			return _entityMapperRegistry.Keys;
        }

        public void SetTableNamePrefix(string tableNamePrefix)
        {
			_tableNamePrefix = tableNamePrefix;
        }

        public void OverrideTableName<T>(string table) where T : new() { 
			OverrideTableName(typeof(T), table);
        }

        public void OverrideTableName(Type entityType, string tableName)
        {
            if (_entityMapperRegistry.ContainsKey(entityType))
            {
                // copy the mapper entry becasue it could be referenced 
                // from parent context
                var duplicatedMapper = new DynamicTableEntityMapper(_entityMapperRegistry[entityType]);

                // override the table name
                duplicatedMapper.TableName = tableName;

                // re-register
                _entityMapperRegistry[entityType] = duplicatedMapper;
            }
        }

        public async Task<bool> ExistsTableAsync<T>()
        {
            var tc = GetTableClient(GetTableName(typeof(T)));
            return await tc.ExistsAsync();
        }

        public async Task CreateTableAsync(Type entityType, bool ignoreErrorIfExists = true) 
		{
			var tc = GetTableClient(GetTableName(entityType));

			if (ignoreErrorIfExists)
				await tc.CreateIfNotExistsAsync();			
			else
				await tc.CreateAsync();			
		}
				
		public Task CreateTableAsync<T>(bool ignoreErrorIfExists = true)
			=> CreateTableAsync(typeof(T), ignoreErrorIfExists);		
		
		public void CreateTable<T>(bool ignoreErrorIfExists = true)
			=> this.CreateTableAsync<T>(ignoreErrorIfExists).GetAwaiter().GetResult();		

        public async Task DropTableAsync(Type entityType, bool ignoreErrorIfNotExists = true) 
        {
            var tc = GetTableClient(GetTableName(entityType));            
            if (ignoreErrorIfNotExists)
                await tc.DeleteIfExistsAsync();
            else
                await tc.DeleteAsync();
        }

        public async Task DropTableAsync<T>(bool ignoreErrorIfNotExists = true)
			=> await DropTableAsync(typeof(T), ignoreErrorIfNotExists);        

        public void DropTable<T>(bool ignoreErrorIfNotExists = true)
			=> Task.Run(async () => await DropTableAsync(typeof(T), ignoreErrorIfNotExists)).Wait();        

		public async Task InsertAsync<T>(IEnumerable<T> models) where T : new ()
			=> await this.StoreAsync(nStoreOperation.insertOperation, models);		

		public async Task MergeAsync<T>(IEnumerable<T> models) where T : new()
			=> await this.StoreAsync(nStoreOperation.mergeOperation, models);	

		public async Task InsertOrReplaceAsync<T>(IEnumerable<T> models) where T : new()
			=> await this.StoreAsync(nStoreOperation.insertOrReplaceOperation, models);	
		
		public async Task InsertOrReplaceAsync<T>(T model) where T : new()
			=> await this.StoreAsync(nStoreOperation.insertOrReplaceOperation, new List<T>() { model });		

		public async Task MergeOrInsertAsync<T>(IEnumerable<T> models) where T : new()
			=> await this.StoreAsync(nStoreOperation.mergeOrInserOperation, models);	
		
		public async Task MergeOrInsertAsync<T>(T model) where T : new()
			=> await this.StoreAsync(nStoreOperation.mergeOrInserOperation, new List<T>() { model });		

        public async Task<T> QueryAsync<T>(string partitionKey, string rowKey, int maxItems = 0) where T : new()
			=> (await QueryAsyncInternal<T>(partitionKey, rowKey, null, maxItems)).FirstOrDefault<T>();				

        public async Task<IQueryable<T>> QueryAsync<T>(string partitionKey, IEnumerable<QueryFilter> queryFilters, int maxItems = 0) where T : new()
			=> await QueryAsyncInternal<T>(partitionKey, null, queryFilters, maxItems);        

        public async Task<IQueryable<T>> QueryAsync<T>(string partitionKey, int maxItems = 0) where T : new()
			=> await QueryAsyncInternal<T>(partitionKey, null, null, maxItems);		

		public async Task<IQueryable<T>> QueryAsync<T>(int maxItems = 0) where T: new()
			=> await QueryAsyncInternal<T>(null, null, null, maxItems);		
		
		private string GetTableName<T>()
			=> GetTableName(typeof(T));		
		
		private string GetTableName(Type entityType)
			=> GetTableName(_entityMapperRegistry[entityType].TableName);        

        private string GetTableName(string tableName)
        {
			// get the table name
			if (String.IsNullOrEmpty(_tableNamePrefix))
				return tableName;
			else
				return Regex.Replace($"{_tableNamePrefix}{tableName}", "[^A-Za-z0-9]", "");
        }

        public async Task StoreAsync<T>(nStoreOperation storaeOperationType, IEnumerable<T> models) where T : new()
		{
			try
			{
				// notify delegate
				if (_delegate != null)
					_delegate.OnStoring(typeof(T), storaeOperationType);


                // Retrieve a reference to the table.
                var tc = GetTableClient(GetTableName<T>());

				// Create the batch
				var tableTransactionsBatch = new List<List<TableTransactionAction>>();

				// Create the frist transaction 
				var tableTransactions = new List<TableTransactionAction>();
				tableTransactionsBatch.Add(tableTransactions);

                // lookup the entitymapper
                var entityMapper = _entityMapperRegistry[typeof(T)];

				// define the modelcounter
				int modelCounter = 0;

				// Add all items
				foreach (var model in models)
				{
					switch (storaeOperationType)
					{
						case nStoreOperation.insertOperation:
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.Add, TableEntityDynamic.ToEntity<T>(model, entityMapper)));
							break;
						case nStoreOperation.insertOrReplaceOperation:
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, TableEntityDynamic.ToEntity<T>(model, entityMapper)));
                            break;
						case nStoreOperation.mergeOperation:
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.UpdateMerge, TableEntityDynamic.ToEntity<T>(model, entityMapper)));
                            break;
						case nStoreOperation.mergeOrInserOperation:
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.UpsertMerge, TableEntityDynamic.ToEntity<T>(model, entityMapper)));
                            break;
						case nStoreOperation.delete:
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.Delete, TableEntityDynamic.ToEntity<T>(model, entityMapper)));
                            break;
					}

					modelCounter++;

					if (modelCounter % 100 == 0)
					{
                        tableTransactions = new List<TableTransactionAction>();
                        tableTransactionsBatch.Add(tableTransactions);
					}
				}

				// execute 
				foreach (var createdBatch in tableTransactionsBatch)
				{
					if (createdBatch.Count() > 0)
					{
						await tc.SubmitTransactionAsync(createdBatch);						

						// notify delegate
						if (_delegate != null)
							_delegate.OnStored(typeof(T), storaeOperationType, createdBatch.Count(), null);
					}
				}
			} 
			catch (TableTransactionFailedException ex) 
			{
				// check the exception
                if (_autoCreateTable && ex.ErrorCode.Equals("TableNotFound"))
                {
				    // try to create the table	
				    await CreateTableAsync<T>();

				    // retry 
				    await StoreAsync<T>(storaeOperationType, models);
                }
				else
				{
					// notify delegate
					if (_delegate != null)
						_delegate.OnStored(typeof(T), storaeOperationType, 0, ex);				
					
					throw ex;
				}				
			}
		}

		public async Task DeleteAsync<T>(T model) where T: new()
			=> await DeleteAsync<T>(new List<T>() { model });		
		
		public async Task DeleteAsync<T>(IEnumerable<T> models, bool allowMultiPartionRemoval = false) where T: new() 
		{
			try
			{
				await this.StoreAsync(nStoreOperation.delete, models);
			} catch(TableTransactionFailedException e)
            {
				if (e.ErrorCode.Equals("CommandsInBatchActOnDifferentPartitions") && allowMultiPartionRemoval)				
                {
					// build a per partition key cache
					var partionKeyDictionary = new Dictionary<string, List<T>>();

					// lookup the entitymapper
					var entityMapper = _entityMapperRegistry[typeof(T)];

					// split our entities
					foreach (var model in models)
					{
						// convert the model to a dynamic entity
						var t = new DynamicTableEntity<T>(model, entityMapper);

						// lookup the partitionkey list
						if (!partionKeyDictionary.ContainsKey(t.PartitionKey))
							partionKeyDictionary.Add(t.PartitionKey, new List<T>());

						// add the model to the list
						partionKeyDictionary[t.PartitionKey].Add(t.Model);
					}

					// remove the different batches
					foreach (var kvp in partionKeyDictionary)
						await DeleteAsync<T>(kvp.Value);
				}
				else
                {
					ExceptionDispatchInfo.Capture(e).Throw();
				}			
            }
		}

		internal async Task<QueryResult<T>> QueryAsyncInternalSinglePage<T>(string partitionKey, string rowKey, IEnumerable<QueryFilter> queryFilters = null, int maxItems = 0, TableContinuationToken continuationToken = null) where T : new()
		{
			try
			{
				// notify delegate
				if (_delegate != null)
					_delegate.OnQuerying(typeof(T), partitionKey, rowKey, maxItems, continuationToken != null);				
					
				// Retrieve a reference to the table.
				var table = GetTableReference(GetTableName<T>());

				// lookup the entitymapper
				var entityMapper = _entityMapperRegistry[typeof(T)];

				// Construct the query to get all entries
				TableQuery<DynamicTableEntity<T>> query = new TableQuery<DynamicTableEntity<T>>();

				// add partitionkey if exists		
				string partitionKeyFilter = null;
				if (partitionKey != null)
					partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);

				// add row key if exists
				string rowKeyFilter = null;
				if (rowKey != null)
					rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);

                // define the max query items
				if (maxItems > 0)
					query = query.Take(maxItems);
					
				// build the query filter
				if (partitionKey != null && rowKey != null)
					query = query.Where(TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter));
				else if (partitionKey != null && rowKey == null)
					query = query.Where(partitionKeyFilter);										
				else if (partitionKey == null && rowKey != null)
					throw new Exception("PartitionKey must have a value");

                // build the final query filter
                if (queryFilters != null)
                {
                    foreach (var queryFilter in queryFilters)
                    {
                        var generatedQueryFilter = queryFilter.ToFilterString();

                        if (String.IsNullOrEmpty(query.FilterString))
                            query.Where(generatedQueryFilter);
                        else if (queryFilter.FilterType == QueryFilterType.Where || queryFilter.FilterType == QueryFilterType.And)
                            query.Where(TableQuery.CombineFilters(query.FilterString, TableOperators.And, generatedQueryFilter));
                        else if (queryFilter.FilterType == QueryFilterType.Or)
                            query.Where(TableQuery.CombineFilters(query.FilterString, TableOperators.Or, generatedQueryFilter));
                    }
                }

                // execute the query											
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
				
				// map all to the original models
				List<T> result = new List<T>();
				foreach (DynamicTableEntity<T> model in queryResult)
					result.Add(model.Model);

				// notify delegate
				if (_delegate != null)
					_delegate.OnQueryed(typeof(T), partitionKey, rowKey, maxItems, continuationToken != null, null);

				// done 
				return new QueryResult<T>() 
				{ 
					Items = result.AsQueryable(), 
					NextToken = queryResult.ContinuationToken 
				};
				
			} catch(Exception e) {

                // check if we have autocreate
                if (_autoCreateTable || e.Message.StartsWith("0:The table specified does not exist", StringComparison.CurrentCulture))
                {

                    // notify delegate
                    if (_delegate != null)
                        _delegate.OnQueryed(typeof(T), partitionKey, rowKey, maxItems, continuationToken != null, null);

                    // done
                    return new QueryResult<T>()
                    {
                        Items = new List<T>().AsQueryable<T>(),
                        NextToken = null
                    };

                }
                else
                {

                    // notify delegate
                    if (_delegate != null)
                        _delegate.OnQueryed(typeof(T), partitionKey, rowKey, maxItems, continuationToken != null, e);

                    // throw exception
                    throw e;
                }
			}
		}
		
		private async Task<IQueryable<T>> QueryAsyncInternal<T>(string partitionKey, string rowKey, IEnumerable<QueryFilter> queryFilters = null, int maxItems = 0, TableContinuationToken nextToken = null) where T : new()
		{			
			// query the first page
			var result = await QueryAsyncInternalSinglePage<T>(partitionKey, rowKey, queryFilters, maxItems, nextToken);
			
			// check if we have reached the max items
			if (maxItems > 0 && result.Items.Count() >= maxItems)
				return result.Items;
			
			if (result.NextToken != null) 						
				return result.Items.Concat(await this.QueryAsyncInternal<T>(partitionKey, rowKey, queryFilters, maxItems, result.NextToken));
			else 			
				return result.Items;					
		}
	
		private CloudTable GetTableReference(string tableName) {
			
			// create the table client 
			var storageTableClient = _storageAccount.CreateCloudTableClient();

			// Create the table client.
			CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

			// Retrieve a reference to the table.
			return tableClient.GetTableReference(tableName);
		}

        private TableClient GetTableClient(string tableName)
        {
			return new TableClient(_connectionString, tableName);            
        }

        internal CloudTable RequestTableReference(string tableName)
        {
			var tableNamePatched = GetTableName(tableName);
			return GetTableReference(tableNamePatched);
        }


        public StorageContextQueryCursor<T> QueryPaged<T>(string partitionKey, string rowKey, IEnumerable<QueryFilter> queryFilters = null, int maxItems = 0) where T : new()
		{
			return new StorageContextQueryCursor<T>(this, partitionKey, rowKey, queryFilters, maxItems);
		}

        public async Task<List<string>> QueryTableList() {

            var tables = new List<string>();

            TableContinuationToken token = null;
            do
            {
                var tableClient = _storageAccount.CreateCloudTableClient();
                var segmentResult = await tableClient.ListTablesSegmentedAsync(token);
                token = segmentResult.ContinuationToken;
                tables.AddRange(segmentResult.Results.Select(t => t.Name));

            } while (token != null);

            return tables;
        }

        public async Task ExportToJsonAsync(string tableName, TextWriter writer)
        {
            var logsTable = GetTableReference(GetTableName(DataExportService.TableName));

			if (!await logsTable.ExistsAsync())
				await logsTable.CreateIfNotExistsAsync();

            var exporter = new DataExportService(this);
            await exporter.ExportToJson(tableName, writer);
        }

        public async Task ImportFromJsonAsync(string tableName, StreamReader reader) 
        {
            var logsTable = GetTableReference(GetTableName(DataImportService.TableName));

            if (!await logsTable.ExistsAsync())
                await logsTable.CreateIfNotExistsAsync();
            
            var importer = new DataImportService(this);
            await importer.ImportFromJsonStreamAsync(tableName, reader);
        }        
    }
}
