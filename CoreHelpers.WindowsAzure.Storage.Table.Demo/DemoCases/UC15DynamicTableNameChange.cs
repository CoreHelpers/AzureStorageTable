using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{

    [Storable]
    public class DemoModel2
    {

        [PartitionKey]
        public string P { get; set; } = "P1";

        [RowKey]
        public string R { get; set; } = "R1";
    }

    public class UC15DynamicTableNameChange : IDemoCase
    {
        public async Task Execute(string connectionString)
        {
            Console.WriteLine("");
            Console.WriteLine(this.GetType().FullName);

            using (var storageContextParent = new StorageContext(connectionString))
            {
                using (var storageContext = new StorageContext(storageContextParent))
                {
                    // create model with data in list
                    var model = new DemoModel2() { P = "1", R = "2" };

                    // ensure we are using the attributes
                    Console.WriteLine("Configuring Entity Mappers");
                    storageContext.AddAttributeMapper(typeof(DemoModel2), "MT1");

                    // inser the model
                    Console.WriteLine("Insert Models");
                    await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DemoModel2>(new List<DemoModel2>() { model });

                    // change table name
                    Console.WriteLine("Update Table Name");
                    storageContext.OverrideTableName<DemoModel2>("MT2");

                    // inser the model
                    Console.WriteLine("Insert Models");
                    await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DemoModel2>(new List<DemoModel2>() { model });

                    // cear table 
                    await storageContext.DropTableAsync<DemoModel2>();
                    storageContext.OverrideTableName<DemoModel2>("MT1");
                    await storageContext.DropTableAsync<DemoModel2>();
                }
            }
        }
    }
}
