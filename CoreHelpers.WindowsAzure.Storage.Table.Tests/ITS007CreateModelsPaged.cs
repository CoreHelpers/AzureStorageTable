using System;
using System.Threading.Tasks;
using System.Linq;
using Xunit.DependencyInjection;
using CoreHelpers.WindowsAzure.Storage.Table.Tests;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
	[Startup(typeof(Startup))]
	[Collection("Sequential")]
	public class ITS007CreateModelsPaged
	{
		private readonly ITestEnvironment env;

		public ITS007CreateModelsPaged(ITestEnvironment env)
		{
			this.env = env;
		}

		[Fact]
		public async Task VerifyPagedWriting()
		{			
			Console.WriteLine("");
			Console.WriteLine(this.GetType().FullName);
						
            using (var storageContext = new StorageContext(env.ConnectionString))
            {     
        		// ensure we are using the attributes				
                storageContext.AddAttributeMapper(typeof(UserModel2), "DemoUserModel2");
                
                // create tables                
				await storageContext.CreateTableAsync<UserModel2>(true);

				// write data pages						
                using (var pagedWriter = new PagedTableEntityWriter<UserModel2>(storageContext, nStoreOperation.insertOrReplaceOperation, 100))
				{
					var t1 = Task.Run(async () =>
					{
						for (var i = 0; i < 500; i++)
						{
							var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = string.Format("em-{0}@acme.org", i) };
							await pagedWriter.StoreAsync(user);
						}
					});

					var t2 = Task.Run(async () =>
					{
						for (var i = 500; i < 1000; i++)
    					{
    						var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = string.Format("em-{0}@acme.org", i) };
    						await pagedWriter.StoreAsync(user);
    					}
					});

					Task.WaitAll(new Task[] { t1, t2 });
				}                

				// query all                
                var result = await storageContext.QueryAsync<UserModel2>();
				Assert.Equal(1000, result.Count());				
				
                // Clean up 				
				await storageContext.DeleteAsync<UserModel2>(result);
				result = await storageContext.QueryAsync<UserModel2>();
				Assert.Equal(0, result.Count());				
            }						
		}

		[Fact]
		public async Task VerifyPageReading()
		{
			using (var storageContext = new StorageContext(env.ConnectionString))
			{
				// ensure we are using the attributes
				storageContext.AddAttributeMapper(typeof(HugeDemoEntry));

				// delete all
				var result = await storageContext.EnableAutoCreateTable().QueryAsync<HugeDemoEntry>();
				await storageContext.DeleteAsync<HugeDemoEntry>(result);

				// create 4000 items				
				var data = new List<HugeDemoEntry>();
				for (int i = 0; i < 4200; i++)
					data.Add(new HugeDemoEntry());

				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<HugeDemoEntry>(data);

				// query items page by page
				var items = new List<HugeDemoEntry>();
				var counter = 0;

				using (var queryCursor = storageContext.QueryPaged<HugeDemoEntry>(null, null))
				{
					while (await queryCursor.LoadNextPageAsync())
					{
						Assert.True(queryCursor.Items.Count() > 0);
						counter += queryCursor.Items.Count();

						items.AddRange(queryCursor.Items);
					}
				}

				Assert.Equal(4200, counter);

				// remove all entries				
				await storageContext.DeleteAsync<HugeDemoEntry>(items);
				result = await storageContext.QueryAsync<HugeDemoEntry>();
				Assert.Equal(0, result.Count());				
			}
		}
	}
}
