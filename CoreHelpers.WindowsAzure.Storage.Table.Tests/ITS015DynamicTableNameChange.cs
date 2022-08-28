using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Abstractions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS015DynamicTableNameChange
    {
        private readonly ITestEnvironment env;

        public ITS015DynamicTableNameChange(ITestEnvironment env)
        {
            this.env = env;
        }

        [Fact]
        public async Task VerifyDynamicNames()
        {
            
            using (var storageContextParent = new StorageContext(env.ConnectionString))
            {
                // set the tablename context
                storageContextParent.SetTableContext();

                using (var storageContext = new StorageContext(storageContextParent))
                {
                    var tableName1 = $"MT1";
                    var tableName2 = $"MT2";

                    // create model with data in list
                    var model = new DemoModel2() { P = "1", R = "2" };

                    // ensure we are using the attributes                    
                    storageContext.AddAttributeMapper(typeof(DemoModel2), tableName1);

                    // inser the model                    
                    await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DemoModel2>(new List<DemoModel2>() { model });

                    // change table name                    
                    storageContext.OverrideTableName<DemoModel2>(tableName2);

                    // inser the model                    
                    await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DemoModel2>(new List<DemoModel2>() { model });

                    // cear table 
                    await storageContext.DropTableAsync<DemoModel2>();
                    storageContext.OverrideTableName<DemoModel2>(tableName1);
                    await storageContext.DropTableAsync<DemoModel2>();
                }
            }
        }
    }
}
