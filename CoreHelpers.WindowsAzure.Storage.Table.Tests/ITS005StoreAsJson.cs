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
	public class ITS005StoreAsJson
	{
		private readonly ITestEnvironment env;

		public ITS005StoreAsJson(ITestEnvironment env)
		{
			this.env = env;
		}

		[Fact]
		public async Task VerifyJsonPayloads()
		{
			var ctx = StorageContextExtensions.BuildTableContext();

            using (var storageContext = new StorageContext(env.ConnectionString))
			{
                // set the tablename context
                storageContext.SetTableNamePrefix(ctx);

                // create the model 
                var model = new JObjectModel() { UUID = "112233" };
				model.Data.Add("HEllo", "world");
				model.Data2.Value = "Hello 23";

				// ensure we are using the attributes				
				storageContext.AddAttributeMapper(typeof(JObjectModel));

				// ensure the table exists                
				await storageContext.CreateTableAsync<JObjectModel>();

				// inser the model                
				await storageContext.MergeOrInsertAsync<JObjectModel>(model);

				// query all				
				var result = await storageContext.QueryAsync<JObjectModel>();
				Assert.Single(result);
				Assert.Equal("112233", result.First().UUID);
				Assert.Single(result.First().Data);
				Assert.True(result.First().Data.ContainsKey("HEllo"));
				Assert.Equal("Hello 23", result.First().Data2.Value);
			}

			using (var storageContext = new StorageContext(env.ConnectionString))
			{
                // set the tablename context
                storageContext.SetTableNamePrefix(ctx);

                // ensure we are using the attributes				
                storageContext.AddAttributeMapper(typeof(JObjectModelVerify));

				var result = await storageContext.QueryAsync<JObjectModelVerify>();
				Assert.Single(result);
				Assert.Equal("112233", result.First().UUID);
				Assert.Equal("{\"Value\":\"Hello 23\"}", result.First().Data2);
				Assert.Equal("{\"HEllo\":\"world\"}", result.First().Data);

			}

			using (var storageContext = new StorageContext(env.ConnectionString))
			{
                // set the tablename context
                storageContext.SetTableNamePrefix(ctx);

                // ensure we are using the attributes				
                storageContext.AddAttributeMapper(typeof(JObjectModel));

				// Clean up
				var result = await storageContext.QueryAsync<JObjectModel>();
				await storageContext.DeleteAsync<JObjectModel>(result);
				result = await storageContext.QueryAsync<JObjectModel>();
				Assert.NotNull(result);
				Assert.Empty(result);

                await storageContext.DropTableAsync<JObjectModel>();
            }
		}

		[Fact]
		public async Task VerifyDictionary()
		{
			using (var storageContext = new StorageContext(env.ConnectionString))
			{
                // set the tablename context
                storageContext.SetTableContext();

                // create the model 
                var model = new DictionaryModel() { Id = Guid.NewGuid().ToString() };

				// ensure we are using the attributes				
				storageContext.AddAttributeMapper(typeof(DictionaryModel));

				// inser the model                
				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DictionaryModel>(model);

				// query all				
				var result = await storageContext.QueryAsync<DictionaryModel>(model.Id, model.Id);
				Assert.Equal(model.Id, result.Id);
				Assert.Empty(model.Propertiers);

				// cleanup				
				var cleanUpItems = await storageContext.QueryAsync<DictionaryModel>();				
				await storageContext.DeleteAsync<DictionaryModel>(cleanUpItems, true);

                await storageContext.DropTableAsync<DictionaryModel>();
            }
		}

		[Fact]
		public async Task DeleteMultiPartitionEntries()
		{
			using (var storageContext = new StorageContext(env.ConnectionString))
			{
                // set the tablename context
                storageContext.SetTableContext();

                // ensure we are using the attributes				
                storageContext.AddAttributeMapper(typeof(DictionaryModel));

				// inser the model                
				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DictionaryModel>(new DictionaryModel() { Id = Guid.NewGuid().ToString() });
				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DictionaryModel>(new DictionaryModel() { Id = Guid.NewGuid().ToString() });
				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DictionaryModel>(new DictionaryModel() { Id = Guid.NewGuid().ToString() });
				
				// cleanup				
				var cleanUpItems = await storageContext.QueryAsync<DictionaryModel>();
				await storageContext.DeleteAsync<DictionaryModel>(cleanUpItems, true);

				var zeroItems = await storageContext.QueryAsync<DictionaryModel>();
				Assert.Empty(zeroItems);

                await storageContext.DropTableAsync<DictionaryModel>();
            }
		}
	}
}
