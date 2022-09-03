using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Abstractions
{
    public interface IStorageContextQueryNow<T>
    {
        Task<IEnumerable<T>> Now();
    }

    public interface IStorageContextQueryWithFilter<T> : IStorageContextQueryNow<T>
    {
        IStorageContextQueryWithFilter<T> Filter(string filter);
        IStorageContextQueryWithFilter<T> Filter(IEnumerable<QueryFilter> filters);        

        IStorageContextQueryWithFilter<T> LimitTo(int maxItems);
    }
   
    public interface IStorageContextQueryWithRowKey<T> : IStorageContextQueryWithFilter<T>
    {
        IStorageContextQueryWithFilter<T> GetItem(string rowKey);
    }

    public interface IStorageContextQueryWithPartitionKey<T> : IStorageContextQueryNow<T>
    {
        IStorageContextQueryWithRowKey<T> InPartition(string partitionKey);

        IStorageContextQueryWithRowKey<T> InAllPartitions();
    }
}

