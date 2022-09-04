using System;
using System.Collections.Generic;
using Azure.Data.Tables;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public partial class StorageContext : IStorageContext
	{						
		private IStorageContextDelegate _delegate { get; set; }		
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
		{}

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
			return new TableClient(_connectionString, tableName);            
        }                                         
    }
}
