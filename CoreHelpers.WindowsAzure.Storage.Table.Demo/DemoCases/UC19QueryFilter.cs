using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Abstractions;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
    [Storable(Tablename: "DemoEntityQuery")]
    public class DemoEntityQuery
    {
        [PartitionKey] public string P { get; set; } = "P1";

        [RowKey] public string R { get; set; } = "R1";

        public string StringField { get; set; }

        public bool BoolField { get; set; }
    }

    public class UC19QueryFilter : IDemoCase
    {
        public async Task Execute(string connectionString)
        {
            // Import from Blob            
            using (var storageContext = new StorageContext(connectionString))
            {
                // create the model 
                var models = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {R = "E1", StringField = "Demo01"},
                    new DemoEntityQuery() {R = "E2", StringField = "Demo02"},
                    new DemoEntityQuery() {R = "E3", StringField = "Demo02"},
                    new DemoEntityQuery() {R = "E4", StringField = "Demo03"},
                    new DemoEntityQuery() {R = "E5", StringField = "Demo03", BoolField = true}
                };

                // ensure we are using the attributes
                Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddAttributeMapper(typeof(DemoEntityQuery));

                // ensure the table exists
                Console.WriteLine("Create Tables");
                await storageContext.CreateTableAsync<DemoEntityQuery>();

                // inser the model
                Console.WriteLine("Insert Models");
                await storageContext.MergeOrInsertAsync<DemoEntityQuery>(models);

                // buidl a filter 
                var queryFilter = new List<QueryFilter>()
                {
                    new QueryFilter()
                    {
                        FilterType = QueryFilterType.Where, 
                        Property = nameof(DemoEntityQuery.StringField),
                        Value = "Demo03", 
                        Operator = QueryFilterOperator.Equal
                    },
                    new QueryFilter()
                    {
                        FilterType = QueryFilterType.And, 
                        Property = nameof(DemoEntityQuery.BoolField),
                        Value = true, 
                        Operator = QueryFilterOperator.Equal
                    },
                    new QueryFilter()
                    {
                        FilterType = QueryFilterType.Or, 
                        Property = nameof(DemoEntityQuery.StringField),
                        Value = "Demo02", 
                        Operator = QueryFilterOperator.Equal
                    },
                };

                // query all
                Console.WriteLine("Query all Models");
                var result = (await storageContext.QueryAsync<DemoEntityQuery>(null, queryFilter)).ToList();

                if (result.Count != 3)
                    Console.WriteLine("Oh no");

                result = (await storageContext.QueryAsync<DemoEntityQuery>("P1", queryFilter)).ToList();

                if (result.Count != 3)
                    Console.WriteLine("Oh no");

                // Clean up 
                Console.WriteLine("Removing all entries");
                var all = await storageContext.QueryAsync<DemoEntityQuery>();
                await storageContext.DeleteAsync<DemoEntityQuery>(all);
            }
        }
    }
}