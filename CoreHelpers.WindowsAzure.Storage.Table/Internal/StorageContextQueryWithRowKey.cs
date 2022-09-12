using System;

namespace CoreHelpers.WindowsAzure.Storage.Table.Internal
{
    public class StorageContextQueryWithRowKey<T> : StorageContextQueryWithFilter<T>, IStorageContextQueryWithRowKey<T> where T : class, new()
    {
        public StorageContextQueryWithRowKey(StorageContext context, StorageContextQueryNow<T> src, string partitionKey)
            : base(context, src, null)
        {
            _optionalPartitionKey = partitionKey;
        }

        public IStorageContextQueryWithFilter<T> GetItem(string rowKey)
        {
            return new StorageContextQueryWithFilter<T>(_context, this, rowKey);
        }
    }
}

