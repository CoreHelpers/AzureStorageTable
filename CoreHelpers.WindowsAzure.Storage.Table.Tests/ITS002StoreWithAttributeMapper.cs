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
        private readonly IStorageContext _rootContext;

        public ITS002StoreWithAttributeMapper(IStorageContext context)
        {
            _rootContext = context;
        }

        [Fact]
        public async Task VerifyAttributeMapper()
        {
            using (var storageContext = _rootContext.CreateChildContext())
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
                Assert.Single(result);
                Assert.Equal("Egon", result.First().FirstName);
                Assert.Equal("Mueller", result.First().LastName);
                Assert.Equal("em@acme.org", result.First().Contact);
                
                // Clean up 
                await storageContext.DeleteAsync<UserModel2>(result);
                result = await storageContext.QueryAsync<UserModel2>();
                Assert.NotNull(result);
                Assert.Empty(result);

                await storageContext.DropTableAsync<UserModel2>();
            }
        }
    }
}

