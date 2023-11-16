using System;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS026RelatedTable
    {
        private readonly IStorageContext _rootContext;

        public ITS026RelatedTable(IStorageContext context)
        {
            _rootContext = context;
        }

        [Fact]
        public async Task ReadRelatedTable()
        {
            using (var storageContext = _rootContext.CreateChildContext())
            {
                // set the tablename context
                storageContext.SetTableContext();
                //
                // create a new user
                var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
                var demo = new DemoModel3() { P = "P2", R = "R2", UserContact = "em@acme.org" };

                // ensure we are using the attributes                
                storageContext.AddAttributeMapper();

                // ensure the tables exists                
                await storageContext.CreateTableAsync<UserModel2>();
                await storageContext.CreateTableAsync<DemoModel3>();

                // inser the models
                await storageContext.MergeOrInsertAsync<UserModel2>(user);
                await storageContext.MergeOrInsertAsync<DemoModel3>(demo);

                // query all                
                var result = await storageContext.QueryAsync<DemoModel3>();
                Assert.Single(result);
                Assert.Equal("Egon", result.First().User?.FirstName);
                Assert.Equal("Mueller", result.First().User?.LastName);
                Assert.Equal("em@acme.org", result.First().User?.Contact);

                // Clean up
                user = result.First().User;
                if (user != null)
                    await storageContext.DeleteAsync<UserModel2>(user);

                var userResult = await storageContext.QueryAsync<UserModel2>();
                Assert.NotNull(userResult);
                Assert.Empty(userResult);


                await storageContext.DeleteAsync<DemoModel3>(result);
                result = await storageContext.QueryAsync<DemoModel3>();
                Assert.NotNull(result);
                Assert.Empty(result);

                await storageContext.DropTableAsync<UserModel2>();
                await storageContext.DropTableAsync<DemoModel3>();
            }
        }


        [Fact]
        public async Task WriteRelatedTable()
        {
            using (var storageContext = _rootContext.CreateChildContext())
            {
                // set the tablename context
                storageContext.SetTableContext();
                //
                // create a new user
                var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
                var demo = new DemoModel4() { P = "P2", R = "R2", UserContact = "em@acme.org", User = user };

                // ensure we are using the attributes                
                storageContext.AddAttributeMapper();

                // ensure the tables exists                
                await storageContext.CreateTableAsync<UserModel2>();
                await storageContext.CreateTableAsync<DemoModel4>();

                // inser the model
                await storageContext.MergeOrInsertAsync<DemoModel4>(demo);

                // query all                
                var result = await storageContext.QueryAsync<DemoModel4>();
                Assert.Single(result);
                Assert.Equal("Egon", result.First().User?.FirstName);
                Assert.Equal("Mueller", result.First().User?.LastName);
                Assert.Equal("em@acme.org", result.First().User?.Contact);

                // Clean up
                user = result.First().User;
                if (user != null)
                    await storageContext.DeleteAsync<UserModel2>(user);

                var userResult = await storageContext.QueryAsync<UserModel2>();
                Assert.NotNull(userResult);
                Assert.Empty(userResult);


                await storageContext.DeleteAsync<DemoModel4>(result);
                result = await storageContext.QueryAsync<DemoModel4>();
                Assert.NotNull(result);
                Assert.Empty(result);

                await storageContext.DropTableAsync<UserModel2>();
                await storageContext.DropTableAsync<DemoModel4>();
            }
        }
    }
}

