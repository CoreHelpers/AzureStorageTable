using System;
using CoreHelpers.WindowsAzure.Storage.Table.Abstractions;

namespace CoreHelpers.WindowsAzure.Storage.Table.Internal
{
    public class StorageContextQueryWithPartitionKey<T> : StorageContextQueryNow<T>, IStorageContextQueryWithPartitionKey<T> where T : class, new()
    {       
        public StorageContextQueryWithPartitionKey(StorageContext context)
        : base(context, null)
        {}

        public IStorageContextQueryWithRowKey<T> InAllPartitions()
        {
            return InPartition(null);
        }

        public IStorageContextQueryWithRowKey<T> InPartition(string partitionKey)
        {
            return new StorageContextQueryWithRowKey<T>(_context, this, partitionKey);
        }
    }
}

