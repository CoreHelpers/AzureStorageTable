using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Abstractions;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public enum ImportExportOperation
    {
        processingItem,
        processingPage,
        processedPage
    }

    public interface IStorageContext : IDisposable
    {
        void AddAttributeMapper();

        void AddAttributeMapper(Type type);

        void AddAttributeMapper(Type type, String optionalTablenameOverride);

        void AddEntityMapper(Type entityType, String partitionKeyFormat, String rowKeyFormat, String tableName);

        IStorageContext CreateChildContext();

        IStorageContext EnableAutoCreateTable();

        bool IsAutoCreateTableEnabled();

        void SetDelegate(IStorageContextDelegate delegateModel);

        IStorageContextDelegate GetDelegate();

        Task InsertOrReplaceAsync<T>(T model) where T : class, new();

        IStorageContextQueryWithPartitionKey<T> Query<T>() where T : class, new();

        Task<T> QueryAsync<T>(string partitionKey, string rowKey, int maxItems = 0) where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T>(string partitionKey, int maxItems = 0) where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T>(string partitionKey, IEnumerable<QueryFilter> queryFilters, int maxItems = 0)
            where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T>(int maxItems = 0) where T : class, new();

        Task DeleteAsync<T>(T model) where T : class, new();

        Task DeleteAsync<T>(IEnumerable<T> models, bool allowMultiPartionRemoval = false) where T : class, new();

        void SetTableNamePrefix(string tableNamePrefix);

        void OverrideTableName<T>(string table) where T : class, new();

        Task MergeOrInsertAsync<T>(IEnumerable<T> models) where T : class, new();

        Task MergeOrInsertAsync<T>(T model) where T : class, new();

        Task CreateTableAsync<T>(bool ignoreErrorIfExists = true);

        void CreateTable<T>(bool ignoreErrorIfExists = true);

        Task<bool> ExistsTableAsync<T>();

        Task DropTableAsync<T>(bool ignoreErrorIfNotExists = true);

        void DropTable<T>(bool ignoreErrorIfNotExists = true);

        Task<List<string>> QueryTableList();

        Task ExportToJsonAsync(string tableName, TextWriter writer, Action<ImportExportOperation> onOperation);

        Task ImportFromJsonAsync(string tableName, StreamReader reader, Action<ImportExportOperation> onOperation);
    }
}