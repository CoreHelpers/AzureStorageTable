using System;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS012PartialUpdateMergeModel
    {
        private readonly ITestEnvironment env;

        public ITS012PartialUpdateMergeModel(ITestEnvironment env)
        {
            this.env = env;
        }

        [Fact]
        public async Task VerifyPartialModelUpdate()
        {
            using (var storageContext = new StorageContext(env.ConnectionString))
            {                     
                // create a new user
                var model = new DemoEntryWithOptionalValues() { Identifier = "X" };            
        
                // ensure we are using the attributes                
                storageContext.AddAttributeMapper();

                // build the table name
                var tableName = $"DemoEntryWithOptinalValues{Guid.NewGuid().ToString().Replace("-", "")}";
                storageContext.OverrideTableName<DemoEntryWithOptionalValues>(tableName);

                // ensure the table exists
                await storageContext.CreateTableAsync<DemoEntryWithOptionalValues>();
        
                // inser the model                
                await storageContext.MergeOrInsertAsync<DemoEntryWithOptionalValues>(model);
        
                // query all                
                var result = (await storageContext.QueryAsync<DemoEntryWithOptionalValues>()).FirstOrDefault();
                Assert.NotNull(result);
                Assert.Equal("X", result.Identifier);
                Assert.Null(result.Name);
                Assert.False(result.Costs.HasValue);
                               
                // update the model
                result.Costs = 5.4;
                await storageContext.MergeOrInsertAsync<DemoEntryWithOptionalValues>(result);
                                       
                // query all                
                result = (await storageContext.QueryAsync<DemoEntryWithOptionalValues>()).FirstOrDefault();
                Assert.NotNull(result);
                Assert.Equal("X", result.Identifier);
                Assert.Null(result.Name);
                Assert.True(result.Costs.HasValue);
                Assert.Equal(5.4, result.Costs.Value);                
                
                // Clean up                 
                await storageContext.DeleteAsync<DemoEntryWithOptionalValues>(result);
                var elements = await storageContext.QueryAsync<DemoEntryWithOptionalValues>();
                Assert.Equal(0, elements.Count());
            }       
        }
    }
}
