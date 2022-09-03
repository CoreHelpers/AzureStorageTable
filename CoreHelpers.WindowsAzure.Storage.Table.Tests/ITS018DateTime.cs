using System;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Tests;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS018DateTime 
    {
        private readonly ITestEnvironment env;

        public ITS018DateTime(ITestEnvironment env)
        {
            this.env = env;
        }

        [Fact]
        public async Task VerifyDateTimeHandling()
        {
            // Import from Blob
            using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // set the tablename context
                storageContext.SetTableContext();

                // create the model 
                var model = new DatetimeModel() { ActivatedAt = DateTime.Now.ToUniversalTime() };

                // save the time
                var dt = model.ActivatedAt;

                // ensure we are using the attributes                
                storageContext.AddAttributeMapper(typeof(DatetimeModel));
                
                // inser the model                
                await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DatetimeModel>(model);

                // query all                
                var result = await storageContext.QueryAsync<DatetimeModel>();
                Assert.Single(result);
                Assert.Equal(dt, result.First().ActivatedAt);
                
                // Clean up                 
                await storageContext.DeleteAsync<DatetimeModel>(result);
                await storageContext.DropTableAsync<DatetimeModel>();
            }
        }
    }
}
