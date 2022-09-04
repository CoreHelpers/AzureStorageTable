using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Azure.Data.Tables;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using System.Threading;
using CoreHelpers.WindowsAzure.Storage.Table.Internal;
using CoreHelpers.WindowsAzure.Storage.Table.Abstractions;

namespace CoreHelpers.WindowsAzure.Storage.Table
{	
	public partial class StorageContext : IStorageContext
	{				
		private Dictionary<Type, StorageEntityMapper> _entityMapperRegistry { get; set; } = new Dictionary<Type, StorageEntityMapper>();
		private bool _autoCreateTable { get; set; } = false;
		private IStorageContextDelegate _delegate { get; set; }
		private string _tableNamePrefix;
		private string _connectionString;

		public StorageContext(string storageAccountName, string storageAccountKey, string storageEndpointSuffix = null)
		{
            _connectionString = String.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2}", "https", storageAccountName, storageAccountKey);
			if (!String.IsNullOrEmpty(storageEndpointSuffix))
                _connectionString = String.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};EndpointSuffix={3}", "https", storageAccountName, storageAccountKey, storageEndpointSuffix);					
		}

        public StorageContext(string connectionString)
        {
			_connectionString = connectionString;
        }

        public StorageContext(StorageContext parentContext)
		{            
            // we reference the entity mapper
            _entityMapperRegistry = new Dictionary<Type, StorageEntityMapper>(parentContext._entityMapperRegistry);

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

		public IStorageContextDelegate GetDelegate()
        {
			return _delegate;
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

        public bool IsAutoCreateTableEnabled()
        {
			return _autoCreateTable;
        }

        public void AddEntityMapper(Type entityType, StorageEntityMapper entityMapper)
		{
			_entityMapperRegistry.Add(entityType, entityMapper);
		}

        public void AddEntityMapper(Type entityType, string partitionKeyFormat, string rowKeyFormat, string tableName)
        {
            _entityMapperRegistry.Add(entityType, new StorageEntityMapper()
			{
				PartitionKeyFormat = partitionKeyFormat,
				RowKeyFormat = rowKeyFormat,
				TableName = tableName
			});
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
            AddEntityMapper(type, new StorageEntityMapper()
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

        public StorageEntityMapper GetEntityMapper<T>()
        {
			return _entityMapperRegistry[typeof(T)];
        }


        public void SetTableNamePrefix(string tableNamePrefix)
        {
			_tableNamePrefix = tableNamePrefix;
        }

        public void OverrideTableName<T>(string table) where T : class, new() { 
			OverrideTableName(typeof(T), table);
        }

        public void OverrideTableName(Type entityType, string tableName)
        {
            if (_entityMapperRegistry.ContainsKey(entityType))
            {
                // copy the mapper entry becasue it could be referenced 
                // from parent context
                var duplicatedMapper = new StorageEntityMapper(_entityMapperRegistry[entityType]);

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

		public async Task InsertAsync<T>(IEnumerable<T> models) where T : class, new()
			=> await this.StoreAsync(nStoreOperation.insertOperation, models);		

		public async Task MergeAsync<T>(IEnumerable<T> models) where T : class, new()
			=> await this.StoreAsync(nStoreOperation.mergeOperation, models);	

		public async Task InsertOrReplaceAsync<T>(IEnumerable<T> models) where T : class, new()
			=> await this.StoreAsync(nStoreOperation.insertOrReplaceOperation, models);	
		
		public async Task InsertOrReplaceAsync<T>(T model) where T : class, new()
			=> await this.StoreAsync(nStoreOperation.insertOrReplaceOperation, new List<T>() { model });		

		public async Task MergeOrInsertAsync<T>(IEnumerable<T> models) where T : class, new()
			=> await this.StoreAsync(nStoreOperation.mergeOrInserOperation, models);	
		
		public async Task MergeOrInsertAsync<T>(T model) where T : class, new()
			=> await this.StoreAsync(nStoreOperation.mergeOrInserOperation, new List<T>() { model });

		public async Task<T> QueryAsync<T>(string partitionKey, string rowKey, int maxItems = 0) where T : class, new()            
            => (await Query<T>().InPartition(partitionKey).GetItem(rowKey).LimitTo(maxItems).Now()).FirstOrDefault<T>();

		public async Task<IEnumerable<T>> QueryAsync<T>(string partitionKey, IEnumerable<QueryFilter> queryFilters, int maxItems = 0) where T : class, new()
			=> await Query<T>().InPartition(partitionKey).Filter(queryFilters).LimitTo(maxItems).Now();			

		public async Task<IEnumerable<T>> QueryAsync<T>(string partitionKey, int maxItems = 0) where T : class, new()
			=> await Query<T>().InPartition(partitionKey).LimitTo(maxItems).Now();

		public async Task<IEnumerable<T>> QueryAsync<T>(int maxItems = 0) where T : class, new()
			=> await Query<T>().InPartition(null).LimitTo(maxItems).Now();			
		
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
				
				// Create the frist transaction 
				var tableTransactions = new List<TableTransactionAction>();

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
						// store the first 100 models
                        await tc.SubmitTransactionWithAutoCreateTableAsync(tableTransactions, default(CancellationToken), _autoCreateTable);

                        // notify delegate
                        if (_delegate != null)
                            _delegate.OnStored(typeof(T), storaeOperationType, tableTransactions.Count(), null);

						// generate a fresh transaction
                        tableTransactions = new List<TableTransactionAction>();                        
					}
				}

				// store the last transaction
				if (tableTransactions.Count > 0)
				{
					await tc.SubmitTransactionWithAutoCreateTableAsync(tableTransactions, default(CancellationToken), _autoCreateTable);

					// notify delegate
					if (_delegate != null)
						_delegate.OnStored(typeof(T), storaeOperationType, tableTransactions.Count(), null);
				}                
			} 
			catch (TableTransactionFailedException ex) 
			{
				// notify delegate
				if (_delegate != null)
					_delegate.OnStored(typeof(T), storaeOperationType, 0, ex);

                ExceptionDispatchInfo.Capture(ex).Throw();
            }
		}

		public async Task DeleteAsync<T>(T model) where T: class, new()
			=> await DeleteAsync<T>(new List<T>() { model });		
		
		public async Task DeleteAsync<T>(IEnumerable<T> models, bool allowMultiPartionRemoval = false) where T: class, new() 
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
						var t = TableEntityDynamic.ToEntity<T>(model, entityMapper);

						// lookup the partitionkey list
						if (!partionKeyDictionary.ContainsKey(t.PartitionKey))
							partionKeyDictionary.Add(t.PartitionKey, new List<T>());

						// add the model to the list
						partionKeyDictionary[t.PartitionKey].Add(model);
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

		public IStorageContextQueryWithPartitionKey<T> Query<T>() where T : class, new()
        {
			return new StorageContextQueryWithPartitionKey<T>(this);
        }
				
        public TableClient GetTableClient<T>()
        {
			var tableName = GetTableName<T>();
			return GetTableClient(tableName);
        }

        private TableClient GetTableClient(string tableName)
        {
			return new TableClient(_connectionString, tableName);            
        }
                
        public async Task<List<string>> QueryTableList() {

            var tables = new List<string>();

			var tsc = new TableServiceClient(_connectionString);
			var tablePages = tsc.QueryAsync().AsPages();

			await foreach(var tablePage in tablePages)
				tables.AddRange(tablePage.Values.Select(t => t.Name));                           
			
            return tables;
        }                  
    }
}
