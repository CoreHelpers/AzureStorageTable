using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Tests;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS003StoreWithAttributeMapperManualRegistration
	{
        private readonly ITestEnvironment env;

        public ITS003StoreWithAttributeMapperManualRegistration(ITestEnvironment env)
        {
            this.env = env;
        }

        [Fact]
        public async Task VerifyManualRegistration()
        {			
			using (var storageContext = new StorageContext(env.ConnectionString))
            {     
        		// create a new user            	
            	var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };            
        		var vpmodel = new VirtualPartKeyDemoModel() { Value1 = "abc", Value2 = "def", Value3 = "ghi" };            
        
                // ensure we are using the attributes                
                storageContext.AddAttributeMapper(typeof(UserModel2));
                storageContext.AddAttributeMapper(typeof(VirtualPartKeyDemoModel));
                
                // ensure the table exists                
                await storageContext.CreateTableAsync<UserModel2>();
                await storageContext.CreateTableAsync<VirtualPartKeyDemoModel>();                
        
                // inser the model                
                await storageContext.MergeOrInsertAsync<UserModel2>(user);
                await storageContext.MergeOrInsertAsync<VirtualPartKeyDemoModel>(vpmodel);
        
                // query all                
                var result = await storageContext.QueryAsync<UserModel2>();
                Assert.Equal(1, result.Count());
                Assert.Equal("Egon", result.First().FirstName);
                Assert.Equal("Mueller", result.First().LastName);
                Assert.Equal("em@acme.org", result.First().Contact);


                var resultVP = await storageContext.QueryAsync<VirtualPartKeyDemoModel>();
                Assert.NotNull(resultVP);
                Assert.Equal(1, resultVP.Count());
                Assert.Equal("abc", resultVP.First().Value1);
                Assert.Equal("def", resultVP.First().Value2);
                Assert.Equal("ghi", resultVP.First().Value3);

                // Clean up 				
				await storageContext.DeleteAsync<UserModel2>(result);
                result = await storageContext.QueryAsync<UserModel2>();
                Assert.NotNull(result);
                Assert.Equal(0, result.Count());

                await storageContext.DeleteAsync<VirtualPartKeyDemoModel>(resultVP);
                resultVP = await storageContext.QueryAsync<VirtualPartKeyDemoModel>();
                Assert.NotNull(resultVP);
                Assert.Equal(0, resultVP.Count());
            }
        }	
	}
}
