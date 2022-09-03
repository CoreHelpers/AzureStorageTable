using System;
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
    public class ITS004GetVirtualArray
	{
        private readonly ITestEnvironment env;

        public ITS004GetVirtualArray(ITestEnvironment env)
        {
            this.env = env;
        }

        [Fact]
		public async Task VerifyVirtualArray()
		{									
            using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // set the tablename context
                storageContext.SetTableContext();

                // create a virtual array model
                var model = new VArrayModel() { UUID = "112233" };
				model.DataElements.Add(2);
				model.DataElements.Add(3);
				model.DataElements.Add(4);
				
				// ensure we are using the attributes				
                storageContext.AddAttributeMapper(typeof(VArrayModel));                
                
                // ensure the table exists                
                await storageContext.CreateTableAsync<VArrayModel>();                
        
                // inser the model                
                await storageContext.MergeOrInsertAsync<VArrayModel>(model);                
        
                // query all                
                var result = await storageContext.QueryAsync<VArrayModel>();
                Assert.Single(result);
                Assert.Equal("112233", result.First().UUID);
                Assert.Equal(3, result.First().DataElements.Count());
                Assert.Equal(2, result.First().DataElements[0]);
                Assert.Equal(3, result.First().DataElements[1]);
                Assert.Equal(4, result.First().DataElements[2]);
                
                // Clean up 				
				await storageContext.DeleteAsync<VArrayModel>(result);
                result = await storageContext.QueryAsync<VArrayModel>();
                Assert.NotNull(result);
                Assert.Empty(result);

                await storageContext.DropTableAsync<VArrayModel>();
            }						
		}	
	}
}
