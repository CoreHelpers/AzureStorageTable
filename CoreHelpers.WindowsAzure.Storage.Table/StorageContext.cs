using System;
using System.Collections.Generic;
using Azure.Data.Tables;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public partial class StorageContext : IStorageContext
    {
        private IStorageContextDelegate _delegate { get; set; }
        private TableServiceClient tableServiceClient { get; set; }

        public StorageContext(string storageAccountName, string storageAccountKey, string storageEndpointSuffix = null)
        {
            if (!String.IsNullOrEmpty(storageEndpointSuffix))
                tableServiceClient = new TableServiceClient(string.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};EndpointSuffix={3}", "https", storageAccountName, storageAccountKey, storageEndpointSuffix));
            else
                tableServiceClient = new TableServiceClient(string.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2}", "https", storageAccountName, storageAccountKey));

        }

        public StorageContext(string connectionString)
        {
            tableServiceClient = new TableServiceClient(connectionString);
        }

        public StorageContext(TableServiceClient tableServiceClient)
        {
            this.tableServiceClient = tableServiceClient;
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
            tableServiceClient = parentContext.tableServiceClient;
        }

        public void Dispose()
        { }

        public void SetDelegate(IStorageContextDelegate delegateModel)
            => _delegate = delegateModel;

        public IStorageContextDelegate GetDelegate()
            => _delegate;

        public IStorageContext CreateChildContext()
            => new StorageContext(this);

        public TableClient GetTableClient<T>()
        {
            var tableName = GetTableName<T>();
            return GetTableClient(tableName);
        }

        private TableClient GetTableClient(string tableName)
        {
            return tableServiceClient.GetTableClient(tableName);
        }
    }
}
