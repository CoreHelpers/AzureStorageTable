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
    public class ITS003StoreWithAttributeMapperManualRegistration
	{
        private readonly IStorageContext _rootContext;

        public ITS003StoreWithAttributeMapperManualRegistration(IStorageContext context)
        {
            _rootContext = context;
        }

        [Fact]
        public async Task VerifyManualRegistration()
        {			
			using (var storageContext = _rootContext.CreateChildContext())
            {
                // set the tablename context
                storageContext.SetTableContext();

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
                Assert.Single(result);
                Assert.Equal("Egon", result.First().FirstName);
                Assert.Equal("Mueller", result.First().LastName);
                Assert.Equal("em@acme.org", result.First().Contact);


                var resultVP = await storageContext.QueryAsync<VirtualPartKeyDemoModel>();
                Assert.NotNull(resultVP);
                Assert.Single(resultVP);
                Assert.Equal("abc", resultVP.First().Value1);
                Assert.Equal("def", resultVP.First().Value2);
                Assert.Equal("ghi", resultVP.First().Value3);

                // Clean up 				
				await storageContext.DeleteAsync<UserModel2>(result);
                result = await storageContext.QueryAsync<UserModel2>();
                Assert.NotNull(result);
                Assert.Empty(result);

                await storageContext.DeleteAsync<VirtualPartKeyDemoModel>(resultVP);
                resultVP = await storageContext.QueryAsync<VirtualPartKeyDemoModel>();
                Assert.NotNull(resultVP);
                Assert.Empty(resultVP);

                await storageContext.DropTableAsync<UserModel2>();
                await storageContext.DropTableAsync<VirtualPartKeyDemoModel>();
            }
        }	
	}
}
