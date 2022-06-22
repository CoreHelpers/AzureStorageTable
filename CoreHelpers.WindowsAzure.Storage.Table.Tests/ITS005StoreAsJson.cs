using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Tests;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
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
			using (var storageContext = new StorageContext(env.ConnectionString))
			{
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
				Assert.Equal(1, result.Count());
				Assert.Equal("112233", result.First().UUID);
				Assert.Equal(1, result.First().Data.Count());
				Assert.True(result.First().Data.ContainsKey("HEllo"));
				Assert.Equal("Hello 23", result.First().Data2.Value);
			}

			using (var storageContext = new StorageContext(env.ConnectionString))
			{
				// ensure we are using the attributes				
				storageContext.AddAttributeMapper(typeof(JObjectModelVerify));

				var result = await storageContext.QueryAsync<JObjectModelVerify>();
				Assert.Equal(1, result.Count());
				Assert.Equal("112233", result.First().UUID);
				Assert.Equal("{\"Value\":\"Hello 23\"}", result.First().Data2);
				Assert.Equal("{\"HEllo\":\"world\"}", result.First().Data);
				
			}

			using (var storageContext = new StorageContext(env.ConnectionString))
			{
				// ensure we are using the attributes				
				storageContext.AddAttributeMapper(typeof(JObjectModel));

				// Clean up
				var result = await storageContext.QueryAsync<JObjectModel>();
				await storageContext.DeleteAsync<JObjectModel>(result);
				result = await storageContext.QueryAsync<JObjectModel>();
				Assert.NotNull(result);
				Assert.Equal(0, result.Count());
			}
		}	
	}
}
