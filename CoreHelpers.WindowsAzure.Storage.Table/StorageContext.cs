using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using HandlebarsDotNet;

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
	
	
	
	public class StorageContext : IDisposable
	{		
		    private CloudStorageAccount _storageAccount { get; set; }
		    private Dictionary<Type, DynamicTableEntityMapper> _entityMapperRegistry { get; set; } = new Dictionary<Type, DynamicTableEntityMapper>();
		    private bool _autoCreateTable { get; set; } = false;
		    private IStorageContextDelegate _delegate { get; set; }
		
		    public StorageContext(string storageAccountName, string storageAccountKey, string storageEndpointSuffix = null)
		    {
			    var connectionString = String.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2}", "https", storageAccountName, storageAccountKey);
			    if (!String.IsNullOrEmpty(storageEndpointSuffix))
				    connectionString = String.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};EndpointSuffix={3}", "https", storageAccountName, storageAccountKey, storageEndpointSuffix);
			
			    _storageAccount = CloudStorageAccount.Parse(connectionString);
		    }

        public StorageContext(string connectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        public StorageContext(StorageContext parentContext)

        {
            _storageAccount = parentContext._storageAccount;
            _entityMapperRegistry = new Dictionary<Type, DynamicTableEntityMapper>(parentContext._entityMapperRegistry);
            this.SetDelegate(parentContext._delegate);
        }

        public void Dispose()
        {

        }

        public void SetDelegate(IStorageContextDelegate delegateModel)
        {
            _delegate = delegateModel;
        }

        public StorageContext EnableAutoCreateTable()
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

            /*foreach(var assembly in Assembly.GetEntryAssembly().GetReferencedAssemblies()) {
                AddAttributeMapper(assembly);
            } */
        }

        internal void AddAttributeMapper(Assembly assembly)
        {
            var typesWithAttribute = assembly.GetTypesWithAttribute(typeof(StorableAttribute));
            foreach (var type in typesWithAttribute)
            {
                AddAttributeMapper(type);
            }
        }

        public void AddAttributeMapper(Type type)
        {
            AddAttributeMapper(type, string.Empty);
        }

        public void AddAttributeMapper(Type type, String optionalTablenameOverride)
        {
            // get the concrete attribute
            var storableAttribute = type.GetTypeInfo().GetCustomAttribute<StorableAttribute>();
            if (String.IsNullOrEmpty(storableAttribute.Tablename))
            {
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

        public void OverrideTableName<T>(string tableName)
        {
            OverrideTableName(typeof(T), tableName);
        }

        public void OverrideTableName(Type entityType, string tableName)
        {
            if (_entityMapperRegistry.ContainsKey(entityType))
                _entityMapperRegistry[entityType].TableName = tableName;
        }

        public Task CreateTableAsync(Type entityType, bool ignoreErrorIfExists = true)
        {
            // Retrieve a reference to the table.
            CloudTable table = GetTableReference(GetTableName(entityType));

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


        public Task CreateTableAsync<T>(bool ignoreErrorIfExists = true)
        {
            return CreateTableAsync(typeof(T), ignoreErrorIfExists);
        }

        public void CreateTable<T>(bool ignoreErrorIfExists = true)
        {
            this.CreateTableAsync<T>(ignoreErrorIfExists).GetAwaiter().GetResult();
        }

        public async Task DropTableAsync(Type entityType, bool ignoreErrorIfNotExists = true)
        {
            // Retrieve a reference to the table.
            CloudTable table = GetTableReference(GetTableName(entityType));

            if (ignoreErrorIfNotExists)
                await table.DeleteIfExistsAsync();
            else
                await table.DeleteAsync();
        }

        public async Task DropTableAsync<T>(bool ignoreErrorIfNotExists = true)
        {
            await DropTableAsync(typeof(T), ignoreErrorIfNotExists);
        }

        public void DropTable<T>(bool ignoreErrorIfNotExists = true)
        {
            Task.Run(async () => await DropTableAsync(typeof(T), ignoreErrorIfNotExists)).Wait();
        }

        public async Task InsertAsync<T>(IEnumerable<T> models) where T : new()
        {
            await this.StoreAsync(nStoreOperation.insertOperation, models);
        }

        public async Task MergeAsync<T>(IEnumerable<T> models) where T : new()
        {
            await this.StoreAsync(nStoreOperation.mergeOperation, models);
        }

        public async Task InsertOrReplaceAsync<T>(IEnumerable<T> models) where T : new()
        {
            await this.StoreAsync(nStoreOperation.insertOrReplaceOperation, models);
        }

        public async Task InsertOrReplaceAsync<T>(T model) where T : new()
        {
            await this.StoreAsync(nStoreOperation.insertOrReplaceOperation, new List<T>() { model });
        }

        public async Task MergeOrInsertAsync<T>(IEnumerable<T> models) where T : new()
        {
            await this.StoreAsync(nStoreOperation.mergeOrInserOperation, models);
        }

        public async Task MergeOrInsertAsync<T>(T model) where T : new()
        {
            await this.StoreAsync(nStoreOperation.mergeOrInserOperation, new List<T>() { model });
        }

        public async Task<T> QueryAsync<T>(string partitionKey, string rowKey, int maxItems = 0) where T : new()
        {
            var result = await QueryAsyncInternal<T>(partitionKey, rowKey, maxItems);
            return result.FirstOrDefault<T>();
        }

        public async Task<IQueryable<T>> QueryAsync<T>(string partitionKey, int maxItems = 0) where T : new()
        {
            return await QueryAsyncInternal<T>(partitionKey, null, maxItems);
        }

        public async Task<IQueryable<T>> QueryAsync<T>(int maxItems = 0) where T : new()
        {
            return await QueryAsyncInternal<T>(null, null, maxItems);
        }


        public async Task<IQueryable<T>> QueryAsyncWithFilter<T>(string filter, string partitionKey) where T : new()
        {
            return await QueryAsyncInternalWithFilter<T>(partitionKey, filter, 0);
        }
        public async Task<IQueryable<T>> QueryAsyncWithFilter<T>(string filter) where T : new()
        {
            return await QueryAsyncInternalWithFilter<T>(null, filter, 0);
        }

        private string GetTableName<T>()
        {
            return GetTableName(typeof(T));
        }

        private string GetTableName(Type entityType)
        {
            // lookup the entitymapper
            var entityMapper = _entityMapperRegistry[entityType];

            // get the table name
            return entityMapper.TableName;
        }

        public async Task StoreAsync<T>(nStoreOperation storaeOperationType, IEnumerable<T> models) where T : new()
        {
            try
            {
                // notify delegate
                if (_delegate != null)
                    _delegate.OnStoring(typeof(T), storaeOperationType);

                // Retrieve a reference to the table.
                CloudTable table = GetTableReference(GetTableName<T>());

                // Create the batch operation.
                List<TableBatchOperation> batchOperations = new List<TableBatchOperation>();

                // Create the first batch
                var currentBatch = new TableBatchOperation();
                batchOperations.Add(currentBatch);

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
                            currentBatch.Insert(new DynamicTableEntity<T>(model, entityMapper));
                            break;
                        case nStoreOperation.insertOrReplaceOperation:
                            currentBatch.InsertOrReplace(new DynamicTableEntity<T>(model, entityMapper));
                            break;
                        case nStoreOperation.mergeOperation:
                            currentBatch.Merge(new DynamicTableEntity<T>(model, entityMapper));
                            break;
                        case nStoreOperation.mergeOrInserOperation:
                            currentBatch.InsertOrMerge(new DynamicTableEntity<T>(model, entityMapper));
                            break;
                        case nStoreOperation.delete:
                            currentBatch.Delete(new DynamicTableEntity<T>(model, entityMapper));
                            break;
                    }

                    modelCounter++;

                    if (modelCounter % 100 == 0)
                    {
                        currentBatch = new TableBatchOperation();
                        batchOperations.Add(currentBatch);
                    }
                }

                // execute 
                foreach (var createdBatch in batchOperations)
                {
                    if (createdBatch.Count() > 0)
                    {
                        await table.ExecuteBatchAsync(createdBatch);

                        // notify delegate
                        if (_delegate != null)
                            _delegate.OnStored(typeof(T), storaeOperationType, createdBatch.Count(), null);
                    }
                }
            }
            catch (StorageException ex)
            {
                // check the exception
                if (!_autoCreateTable || !ex.Message.StartsWith("0:The table specified does not exist", StringComparison.CurrentCulture))
                {
                    // notify delegate
                    if (_delegate != null)
                        _delegate.OnStored(typeof(T), storaeOperationType, 0, ex);

                    throw ex;
                }

                // try to create the table	
                await CreateTableAsync<T>();

                // retry 
                await StoreAsync<T>(storaeOperationType, models);
            }
        }

        public async Task DeleteAsync<T>(T model) where T : new()
        {
            await this.StoreAsync(nStoreOperation.delete, new List<T>() { model });
        }

        public async Task DeleteAsync<T>(IEnumerable<T> models) where T : new()
        {
            await this.StoreAsync(nStoreOperation.delete, models);
        }

        internal async Task<QueryResult<T>> QueryAsyncInternalSinglePage<T>(string partitionKey, string rowKey, int maxItems = 0, TableContinuationToken continuationToken = null, string filter = null) where T : new()
        {
            try
            {
                // notify delegate
                if (_delegate != null)
                    _delegate.OnQuerying(typeof(T), partitionKey, rowKey, maxItems, continuationToken != null);

                // exit early if partitionKey is unspecified
                if (partitionKey == null && rowKey != null)
                    throw new Exception("PartitionKey must have a value if RowKey is specified");

                // Retrieve a reference to the table.
                CloudTable table = GetTableReference(GetTableName<T>());

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

                string where = partitionKeyFilter;
                // build the query filter
                if (rowKey != null)
                    where = TableQueryEx.CheckAndCombineFilters(where, TableOperators.And, rowKeyFilter);
                if (filter != null)
                    where = TableQueryEx.CheckAndCombineFilters(where, TableOperators.And, filter);


                query = query.Where(where);

                // execute the query											
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                // map all to the original models
                var result = new List<T>();
                var relatedItems = new List<RelatedTableItem<T>>();

                foreach (DynamicTableEntity<T> model in queryResult)
                {
                    // find associate items in related tables
                    var relatedItem = LoadRelatedTables(model);
                    if (relatedItem != null)
                        relatedItems.Add(relatedItem);

                    result.Add(model.Model);
                }

                // load related items
                await LoadEagerRelatedItems(relatedItems);

                // notify delegate
                if (_delegate != null)
                    _delegate.OnQueryed(typeof(T), partitionKey, rowKey, maxItems, continuationToken != null, null);

                // done 
                return new QueryResult<T>()
                {
                    Items = result.AsQueryable(),
                    NextToken = queryResult.ContinuationToken
                };

            }
            catch (Exception e)
            {

                // notify delegate
                if (_delegate != null)
                    _delegate.OnQueryed(typeof(T), partitionKey, rowKey, maxItems, continuationToken != null, e);

                // throw exception
                throw e;
            }
        }


        private async Task<IQueryable<T>> QueryAsyncInternal<T>(string partitionKey, string rowKey, int maxItems = 0, TableContinuationToken nextToken = null) where T : new()
        {
            // query the first page
            var result = await QueryAsyncInternalSinglePage<T>(partitionKey, rowKey, maxItems, nextToken);

            // check if we have reached the max items
            if (maxItems > 0 && result.Items.Count() >= maxItems)
                return result.Items;

            if (result.NextToken != null)
                return result.Items.Concat(await this.QueryAsyncInternal<T>(partitionKey, rowKey, maxItems, result.NextToken));
            else
                return result.Items;
        }
        private async Task<IQueryable<T>> QueryAsyncInternalWithFilter<T>(string partitionKey, string filter, int maxItems = 0, TableContinuationToken nextToken = null) where T : new()
        {
            // query the first page
            var result = await QueryAsyncInternalSinglePage<T>(partitionKey, null, maxItems, nextToken, filter);

            // check if we have reached the max items
            if (maxItems > 0 && result.Items.Count() >= maxItems)
                return result.Items;

            if (result.NextToken != null)
                return result.Items.Concat(await this.QueryAsyncInternalWithFilter<T>(partitionKey, filter, maxItems, result.NextToken));
            else
                return result.Items;
        }

        private CloudTable GetTableReference(string tableName)
        {

            // create the table client 
            var storageTableClient = _storageAccount.CreateCloudTableClient();

            // Create the table client.
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            return tableClient.GetTableReference(tableName);
        }


        public StorageContextQueryCursor<T> QueryPaged<T>(string partitionKey, string rowKey, int maxItems = 0) where T : new()
        {
            return new StorageContextQueryCursor<T>(this, partitionKey, rowKey, maxItems);
        }

        private RelatedTableItem<T> LoadRelatedTables<T>(DynamicTableEntity<T> model) where T : new()
        {
            IEnumerable<PropertyInfo> objectProperties = model.Model.GetType().GetTypeInfo().GetProperties();

            foreach (PropertyInfo property in objectProperties)
            {
                if (property.GetCustomAttribute<RelatedTableAttribute>() != null)
                {
                    var relatedTable = property.GetCustomAttribute<RelatedTableAttribute>();

                    Type endType;
                    if (property.PropertyType.IsGenericOfType(typeof(Lazy<>)))
                    {
                        endType = property.PropertyType.GetTypeInfo().GenericTypeArguments[0];
                    }
                    else
                    {
                        endType = property.PropertyType;
                    }

                    // determine the partition key
                    string extPartition = relatedTable.PartitionKey;
                    if (!string.IsNullOrWhiteSpace(extPartition))
                    {
                        // if the partition key is the name of a property on the model, get the value
                        var partitionProperty = objectProperties.Where((pi) => pi.Name == relatedTable.PartitionKey).FirstOrDefault();
                        if (partitionProperty != null)
                        {
                            extPartition = partitionProperty.GetValue(model.Model).ToString();
                        }
                    }

                    string extRowKey = relatedTable.RowKey;
                    if (!string.IsNullOrWhiteSpace(extRowKey))
                    {
                        // if the row key is the name of a property on the model, get the value
                        var rowkeyProperty = objectProperties.Where((pi) => pi.Name == relatedTable.PartitionKey).FirstOrDefault();
                        if (rowkeyProperty != null)
                        {
                            extRowKey = rowkeyProperty.GetValue(model.Model).ToString();
                        }
                    }
                    else
                    {
                        // if the type of the object is the name of a property on the model, get the value of that property as the rowkey
                        var rowkeyProperty = objectProperties.Where((pi) => pi.Name == endType.Name).FirstOrDefault();
                        if (rowkeyProperty != null)
                        {
                            extRowKey = rowkeyProperty.GetValue(model.Model).ToString();
                        }
                    }

                    // make a dynamic reference to the query method
                    var method = typeof(StorageContext).GetMethod(nameof(QueryAsync), new[] { typeof(string), typeof(string), typeof(int) });
                    var generic = method.MakeGenericMethod(endType);

                    // if the property is a lazy type, create the lazy initialization
                    if (property.PropertyType.IsGenericOfType(typeof(Lazy<>)))
                    {
                        var lazyType = typeof(DynamicLazy<>);
                        var constructed = lazyType.MakeGenericType(endType);

                        object o = Activator.CreateInstance(constructed, new Func<object>(() =>
                        {
                            var waitable = (dynamic)generic.Invoke(this, new object[] { extPartition, extRowKey, 1 });
                            var r = waitable.Result;

                            return r;
                        }));
                        property.SetValue(model.Model, o);

                    }
                    else
                    {
                        // return a related table item, in order to optimize the eager loading
                        return new RelatedTableItem<T>()
                        {
                            RowKey = extRowKey,
                            PartitionKey = extPartition,
                            Model = model,
                            Property = property
                        };
                    }
                }
            }
            return null;
        }

        private async Task LoadEagerRelatedItems<T>(List<RelatedTableItem<T>> relatedItems) where T : new()
        {

            var method = typeof(StorageContext).GetMethod(nameof(QueryAsyncWithFilter), new[] { typeof(string), typeof(string) });

            //group by the property type (same table)
            var relatedItemPropertyGroups = relatedItems.GroupBy((i) => i.Property.PropertyType);
            foreach (var relatedItemPropertyGroup in relatedItemPropertyGroups)
            {
                // group by partition key for better query performance
                var relatedItemGroups = relatedItemPropertyGroup.GroupBy((i) => i.PartitionKey);
                var propertyType = relatedItemPropertyGroup.Key;
                var generic = method.MakeGenericMethod(propertyType);

                // lookup the entitymapper
                var entityMapper = _entityMapperRegistry[propertyType];

                foreach (var relatedItemGroup in relatedItemGroups)
                {
                    // create a rowkey filter
                    var rowKeysFilter = TableQueryEx.CombineFilters(
                        relatedItemGroup.Select((i) => TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, i.RowKey)),
                        TableOperators.Or
                    );

                    // query dynamically
                    var task = (Task)generic.Invoke(this, new object[] { rowKeysFilter, relatedItemGroup.Key });
                    await task;

                    var r = (System.Collections.IEnumerable)((dynamic)task).Result;
                    foreach (var item in r)
                    {
                        string rowKey = "";

                        // get the rowkey
                        // we have the partition key because we have grouped by it before 
                        if (entityMapper.RowKeyFormat.Contains("{{") && entityMapper.RowKeyFormat.Contains("}}"))
                        {
                            var template = Handlebars.Compile(entityMapper.RowKeyFormat);
                            rowKey= template(item);
                        }
                        else
                        {
                            var propertyInfo = item.GetType().GetRuntimeProperty(entityMapper.RowKeyFormat);
                            rowKey = propertyInfo.GetValue(item) as String;
                        }


                        // find a set models which reference the item (the same item can be referenced from multiple models)
                        var models = relatedItemGroup.Where((i) => i.RowKey == rowKey && i.PartitionKey == relatedItemGroup.Key);
                        foreach (var model in models)
                        {
                            model.Property.SetValue(model.Model.Model, item);
                        }

                    }
                }
            }
        }
    }
}
