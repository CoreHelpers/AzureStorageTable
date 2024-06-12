using System;
using Azure.Data.Tables;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public partial class StorageContext : IStorageContext
    {
        public async Task DeleteAsync<T>(T model) where T : class, new()
             => await DeleteAsync<T>(new List<T>() { model });

        public async Task DeleteAsync<T>(IEnumerable<T> models, bool allowMultiPartionRemoval = false) where T : class, new()
        {
            try
            {
                await this.StoreAsync(nStoreOperation.delete, models);
            }
            catch (TableTransactionFailedException e)
            {
                if (e.ErrorCode.Equals("CommandsInBatchActOnDifferentPartitions") && allowMultiPartionRemoval)
                {
                    // build a per partition key cache
                    var partionKeyDictionary = new Dictionary<string, List<T>>();

                    // lookup the entitymapper
                    var entityMapper = _entityMapperRegistry[typeof(T)];

                    // split our entities
                    foreach (var model in models)
                    {
                        // convert the model to a dynamic entity
                        var t = TableEntityDynamic.ToEntity<T>(model, entityMapper, this);

                        // lookup the partitionkey list
                        if (!partionKeyDictionary.ContainsKey(t.PartitionKey))
                            partionKeyDictionary.Add(t.PartitionKey, new List<T>());

                        // add the model to the list
                        partionKeyDictionary[t.PartitionKey].Add(model);
                    }

                    // remove the different batches
                    foreach (var kvp in partionKeyDictionary)
                        await DeleteAsync<T>(kvp.Value);
                }
                else
                {
                    ExceptionDispatchInfo.Capture(e).Throw();
                }
            }
        }
    }
}