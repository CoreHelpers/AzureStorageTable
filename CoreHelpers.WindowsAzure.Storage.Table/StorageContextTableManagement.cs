using System;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Azure.Data.Tables;
using System.Collections.Generic;
using System.Linq;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public partial class StorageContext : IStorageContext
    {
        private bool _autoCreateTable { get; set; } = false;
        private string _tableNamePrefix;

        public bool IsAutoCreateTableEnabled()
            => _autoCreateTable;

        public IStorageContext EnableAutoCreateTable()
        {
            _autoCreateTable = true;
            return this;
        }
        
        public void SetTableNamePrefix(string tableNamePrefix)
        {
            _tableNamePrefix = tableNamePrefix;
        }

        public string GetTableNamePrefix()
            => _tableNamePrefix;

        public void OverrideTableName<T>(string table) where T : class, new()
        {
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

        public async Task<List<string>> QueryTableList()
        {
            var tables = new List<string>();

            var tsc = new TableServiceClient(_connectionString);
            var tablePages = tsc.QueryAsync().AsPages();

            await foreach (var tablePage in tablePages)
                tables.AddRange(tablePage.Values.Select(t => t.Name));

            return tables;
        }
    }
}

