using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS013DynamicallyCreateList
    {
        private readonly ITestEnvironment env;

        public ITS013DynamicallyCreateList(ITestEnvironment env)
        {
            this.env = env;
        }

        [Fact]
        public async Task VerifyDynamicLists()
        {
            using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // create model with data in list
                var model = new DemoMeterModel() { ExtendedCosts = new List<Double>() };
                model.ExtendedCosts.Add(5.5);
                model.ExtendedCosts.Add(6.0);

                var model2 = new DemoMeterModel() { R = "R2" };

                // ensure we are using the attributes                
                storageContext.AddAttributeMapper(typeof(DemoMeterModel));

                // ensure the table exists                
                await storageContext.CreateTableAsync<DemoMeterModel>();
        
                // inser the model                
                await storageContext.MergeOrInsertAsync<DemoMeterModel>(model);
                await storageContext.MergeOrInsertAsync<DemoMeterModel>(model2);
        
                // query all                
                var result = await storageContext.QueryAsync<DemoMeterModel>();
                Assert.Equal(2, result.Count());
                Assert.Equal(5.5, result.First().ExtendedCosts[0]);
                Assert.Equal(6.0, result.First().ExtendedCosts[1]);
                Assert.Null(result.Last().ExtendedCosts);

                // Clean up                 
                await storageContext.DeleteAsync<DemoMeterModel>(result);
                var elements = await storageContext.QueryAsync<DemoMeterModel>();
                Assert.Equal(0, elements.Count());
            }       
        }

        [Fact]
        public async Task VerifyNullListWriting()
        {
            using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // create model with data in list
                var model = new NullListModel();

                // ensure we are using the attributes                
                storageContext.AddAttributeMapper(typeof(NullListModel));

                // build the table name
                var tableName = $"NullListModel{Guid.NewGuid().ToString().Replace("-", "")}";
                storageContext.OverrideTableName<NullListModel>(tableName);

                // ensure the table exists                
                await storageContext.CreateTableAsync<NullListModel>();

                // inser the model                
                await storageContext.MergeOrInsertAsync<NullListModel>(model);

                // query all                
                var result = await storageContext.QueryAsync<NullListModel>();
                Assert.Equal(1, result.Count());
                Assert.Null(result.First().Items);                

                // Clean up                 
                await storageContext.DeleteAsync<NullListModel>(result);
                var elements = await storageContext.QueryAsync<NullListModel>();
                Assert.Equal(0, elements.Count());
            }
        }
        
    }
}
