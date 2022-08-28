using System;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS002StoreWithAttributeMapper
    {
        private readonly ITestEnvironment env;

        public ITS002StoreWithAttributeMapper(ITestEnvironment env)
        {
            this.env = env;
        }

        [Fact]
        public async Task VerifyAttributeMapper()
        {
            using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // set the tablename context
                storageContext.SetTableContext();

                // create a new user
                var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };

                // ensure we are using the attributes                
                storageContext.AddAttributeMapper();

                // ensure the table exists                
                await storageContext.CreateTableAsync<UserModel2>();

                // inser the model                
                await storageContext.MergeOrInsertAsync<UserModel2>(user);

                // query all                
                var result = await storageContext.QueryAsync<UserModel2>();
                Assert.Equal(1, result.Count());
                Assert.Equal("Egon", result.First().FirstName);
                Assert.Equal("Mueller", result.First().LastName);
                Assert.Equal("em@acme.org", result.First().Contact);
                
                // Clean up 
                await storageContext.DeleteAsync<UserModel2>(result);
                result = await storageContext.QueryAsync<UserModel2>();
                Assert.NotNull(result);
                Assert.Equal(0, result.Count());

                await storageContext.DropTableAsync<UserModel2>();
            }
        }
    }
}

