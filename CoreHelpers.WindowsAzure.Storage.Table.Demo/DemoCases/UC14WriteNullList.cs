using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
    [Storable]
    public class NullListModel {

        [PartitionKey]
        public string P { get; set; } = "P1";

        [RowKey]
        public string R { get; set; } = "R1";

        [StoreAsJsonObject(typeof(List<string>))]
        public List<string> Items { get; set; }
    }

    public class UC14WriteNullList : IDemoCase
    {
        public async Task Execute(string storageKey, string storageSecret, string endpointSuffix = null)
        {
            Console.WriteLine("");
            Console.WriteLine(this.GetType().FullName);

            using (var storageContext = new StorageContext(storageKey, storageSecret, endpointSuffix))
            {
                // create model with data in list
                var model = new NullListModel();

                // ensure we are using the attributes
                Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddAttributeMapper(typeof(NullListModel));

                // ensure the table exists
                Console.WriteLine("Create Tables");
                await storageContext.CreateTableAsync<NullListModel>();

                // inser the model
                Console.WriteLine("Insert Models");
                await storageContext.MergeOrInsertAsync<NullListModel>(model);

                // query all
                Console.WriteLine("Query all Models");
                var result = await storageContext.QueryAsync<NullListModel>();

                foreach (var item in result)
                {
                    if (item.Items == null)
                    {
                        Console.WriteLine("Items are null");
                    } else {
                        Console.WriteLine("Items are not null");
                    }
                }

                // Clean up 
                Console.WriteLine("Removing all entries");
                await storageContext.DeleteAsync<NullListModel>(result);
            }
        }
    }
}
