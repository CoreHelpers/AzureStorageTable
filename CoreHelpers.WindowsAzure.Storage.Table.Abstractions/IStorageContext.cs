using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Abstractions
{
    public interface IStorageContext : IDisposable
    {
        void AddAttributeMapper(Type type);

        IStorageContext CreateChildContext();

        IStorageContext EnableAutoCreateTable();

        Task InsertOrReplaceAsync<T>(T model) where T : new();

        Task<T> QueryAsync<T>(string partitionKey, string rowKey, int maxItems = 0) where T : new();

        Task<IQueryable<T>> QueryAsync<T>(string partitionKey, int maxItems = 0) where T : new();

        Task<IQueryable<T>> QueryAsync<T>(string partitionKey, IEnumerable<QueryFilter> queryFilters, int maxItems = 0)
            where T : new();

        Task<IQueryable<T>> QueryAsync<T>(int maxItems = 0) where T : new();
        
        Task DeleteAsync<T>(T model) where T : new();

        Task DeleteAsync<T>(IEnumerable<T> models, bool allowMultiPartionRemoval = false) where T : new();

        void OverrideTableName<T>(string table) where T : new();

        Task MergeOrInsertAsync<T>(IEnumerable<T> models) where T : new();

        Task MergeOrInsertAsync<T>(T model) where T : new();
    }
}