using System;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Tests;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Microsoft.WindowsAzure.Storage;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS006AutoCreateTable
    {
        private readonly ITestEnvironment env;

        public ITS006AutoCreateTable(ITestEnvironment env)
        {
            this.env = env;
        }

        [Fact]
        public async Task VerifyAutoCreateDuringWrite()
        {

            using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // create a new user
                var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };

                // generate tablename                
                var tableName = $"T{Guid.NewGuid().ToString()}".Replace("-", "");

                // ensure we are using the attributes                
                storageContext.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = tableName, PartitionKeyFormat = "Contact", RowKeyFormat = "Contact" });

                // inser the model and do not create table
                Assert.Throws<AggregateException>(() => storageContext.MergeOrInsertAsync<UserModel>(user).Wait());

                // inser the model and create table
                await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<UserModel>(user);

                // query all                
                var result = await storageContext.QueryAsync<UserModel>();
                Assert.Equal(1, result.Count());
                Assert.Equal("Egon", result.First().FirstName);
                Assert.Equal("Mueller", result.First().LastName);
                Assert.Equal("em@acme.org", result.First().Contact);

                // Clean up 
                await storageContext.DeleteAsync<UserModel>(result);
                await storageContext.DropTableAsync<UserModel>();
            }
        }

        [Fact]
        public async Task VerifyAutoCreateDuringRead()
        {
            using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // create a new user
                var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };

                // generate tablename                
                var tableName = $"T{Guid.NewGuid().ToString()}".Replace("-", "");

                // ensure we are using the attributes                
                storageContext.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = tableName, PartitionKeyFormat = "Contact", RowKeyFormat = "Contact" });

                // query all and expect exception
                Assert.Throws<AggregateException>(() => storageContext.QueryAsync<UserModel>().Wait());

                // query all by creating a new table
                var result = await storageContext.EnableAutoCreateTable().QueryAsync<UserModel>();
                Assert.Equal(0, result.Count());
            }
        }
    }
}