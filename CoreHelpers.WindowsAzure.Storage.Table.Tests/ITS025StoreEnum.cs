using System;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS025StoreEnum
    {
        private readonly IStorageContext _rootContext;

        public ITS025StoreEnum(IStorageContext context)
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
                var user = new UserModel4() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org", UserType = UserTypeEnum.Pro };

                // ensure we are using the attributes                
                storageContext.AddAttributeMapper();

                // ensure the table exists                
                await storageContext.CreateTableAsync<UserModel4>();

                // inser the model                
                await storageContext.MergeOrInsertAsync<UserModel4>(user);

                // query all                
                var result = await storageContext.QueryAsync<UserModel4>();
                Assert.Single(result);
                Assert.Equal("Egon", result.First().FirstName);
                Assert.Equal("Mueller", result.First().LastName);
                Assert.Equal("em@acme.org", result.First().Contact);
                Assert.Equal(UserTypeEnum.Pro, result.First().UserType);

                // Clean up 
                await storageContext.DeleteAsync<UserModel4>(result);
                result = await storageContext.QueryAsync<UserModel4>();
                Assert.NotNull(result);
                Assert.Empty(result);

                await storageContext.DropTableAsync<UserModel4>();
            }
        }
    }
}

