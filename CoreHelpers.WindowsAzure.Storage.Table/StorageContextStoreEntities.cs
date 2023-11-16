using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public partial class StorageContext : IStorageContext
    {
        public async Task InsertAsync<T>(IEnumerable<T> models) where T : class, new()
            => await this.StoreAsync(nStoreOperation.insertOperation, models);

        public async Task MergeAsync<T>(IEnumerable<T> models) where T : class, new()
            => await this.StoreAsync(nStoreOperation.mergeOperation, models);

        public async Task InsertOrReplaceAsync<T>(IEnumerable<T> models) where T : class, new()
            => await this.StoreAsync(nStoreOperation.insertOrReplaceOperation, models);

        public async Task InsertOrReplaceAsync<T>(T model) where T : class, new()
            => await this.StoreAsync(nStoreOperation.insertOrReplaceOperation, new List<T>() { model });

        public async Task MergeOrInsertAsync<T>(IEnumerable<T> models) where T : class, new()
            => await this.StoreAsync(nStoreOperation.mergeOrInserOperation, models);

        public async Task MergeOrInsertAsync<T>(T model) where T : class, new()
            => await this.StoreAsync(nStoreOperation.mergeOrInserOperation, new List<T>() { model });

        public async Task StoreAsync<T>(nStoreOperation storaeOperationType, IEnumerable<T> models) where T : new()
        {
            try
            {
                // notify delegate
                if (_delegate != null)
                    _delegate.OnStoring(typeof(T), storaeOperationType);


                // Retrieve a reference to the table.
                var tc = GetTableClient(GetTableName<T>());

                // Create the frist transaction 
                var tableTransactions = new List<TableTransactionAction>();

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
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.Add, TableEntityDynamic.ToEntity<T>(model, entityMapper, this)));
                            break;
                        case nStoreOperation.insertOrReplaceOperation:
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, TableEntityDynamic.ToEntity<T>(model, entityMapper, this)));
                            break;
                        case nStoreOperation.mergeOperation:
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.UpdateMerge, TableEntityDynamic.ToEntity<T>(model, entityMapper, this)));
                            break;
                        case nStoreOperation.mergeOrInserOperation:
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.UpsertMerge, TableEntityDynamic.ToEntity<T>(model, entityMapper, this)));
                            break;
                        case nStoreOperation.delete:
                            tableTransactions.Add(new TableTransactionAction(TableTransactionActionType.Delete, TableEntityDynamic.ToEntity<T>(model, entityMapper, this)));
                            break;
                    }

                    modelCounter++;

                    if (modelCounter % 100 == 0)
                    {
                        // store the first 100 models
                        await tc.SubmitTransactionWithAutoCreateTableAsync(tableTransactions, default(CancellationToken), _autoCreateTable);

                        // notify delegate
                        if (_delegate != null)
                            _delegate.OnStored(typeof(T), storaeOperationType, tableTransactions.Count(), null);

                        // generate a fresh transaction
                        tableTransactions = new List<TableTransactionAction>();
                    }
                }

                // store the last transaction
                if (tableTransactions.Count > 0)
                {
                    await tc.SubmitTransactionWithAutoCreateTableAsync(tableTransactions, default(CancellationToken), _autoCreateTable);

                    // notify delegate
                    if (_delegate != null)
                        _delegate.OnStored(typeof(T), storaeOperationType, tableTransactions.Count(), null);
                }
            }
            catch (TableTransactionFailedException ex)
            {
                // notify delegate
                if (_delegate != null)
                    _delegate.OnStored(typeof(T), storaeOperationType, 0, ex);

                ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }
    }
}