using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Abstractions.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Abstractions
{
    public interface IStorageContext : IDisposable
    {
        IEnumerable<Type> RegisteredMappers { get; }

        void AddAttributeMapper();
        void AddAttributeMapper(Type type);
        void AddAttributeMapper(Type type, string optionalTablenameOverride);

        void AddEntityMapper(Type entityType, DynamicTableEntityMapper entityMapper);

        void CreateTable<T>(bool ignoreErrorIfExists = true);

        Task CreateTableAsync<T>(bool ignoreErrorIfExists = true);
        Task CreateTableAsync(Type entityType, bool ignoreErrorIfExists = true);

        Task DeleteAsync<T>(T model) where T : new();
        Task DeleteAsync<T>(IEnumerable<T> models) where T : new();
        
        void DropTable<T>(bool ignoreErrorIfNotExists = true);

        Task DropTableAsync<T>(bool ignoreErrorIfNotExists = true);
        Task DropTableAsync(Type entityType, bool ignoreErrorIfNotExists = true);

        IStorageContext EnableAutoCreateTable();

        Task ExportToJsonAsync(string tableName, TextWriter writer);

        Task ImportFromJsonAsync(string tableName, StreamReader reader);
        
        Task InsertAsync<T>(IEnumerable<T> models) where T : new();

        Task InsertOrReplaceAsync<T>(T model) where T : new();
        Task InsertOrReplaceAsync<T>(IEnumerable<T> models) where T : new();

        void RemoveEntityMapper(Type entityType);

        Task MergeAsync<T>(IEnumerable<T> models) where T : new();

        Task MergeOrInsertAsync<T>(T model) where T : new();
        Task MergeOrInsertAsync<T>(IEnumerable<T> models) where T : new();

        void OverrideTableName<T>(string tableName);
        void OverrideTableName(Type entityType, string tableName);

        Task<T> QueryAsync<T>(string partitionKey, string rowKey, int maxItems = 0) where T : new();
        Task<IQueryable<T>> QueryAsync<T>(string partitionKey, int maxItems = 0) where T : new();

        Task<IQueryable<T>> QueryAsync<T>(string partitionKey, IEnumerable<QueryFilter> queryFilters, int maxItems = 0)
            where T : new();

        Task<IQueryable<T>> QueryAsync<T>(int maxItems = 0) where T : new();

        IStorageContextQueryCursor<T> QueryPaged<T>(string partitionKey, string rowKey,
            IEnumerable<QueryFilter> queryFilters = null, int maxItems = 0) where T : new();

        Task<IEnumerable<string>> QueryTableList();

        void SetDelegate(IStorageContextDelegate delegateModel);

        Task StoreAsync<T>(StorageOperation storageOperation, IEnumerable<T> models) where T : new();
    }
}