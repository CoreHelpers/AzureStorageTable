using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Xunit.DependencyInjection;
using CoreHelpers.WindowsAzure.Storage.Table.Tests;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
	[Startup(typeof(Startup))]
	[Collection("Sequential")]
	public class ITS008CheckMaxItems
	{
		private readonly ITestEnvironment env;

		public ITS008CheckMaxItems(ITestEnvironment env)
		{
			this.env = env;
		}

		[Fact]
		public async Task VerifyMaxItems()
		{			
			Console.WriteLine("");
			Console.WriteLine(this.GetType().FullName);
						
            using (var storageContext = new StorageContext(env.ConnectionString))
            {             		 			     	
				storageContext.AddAttributeMapper(typeof(UserModel2), "DemoUserModel2");								
				storageContext.CreateTable<UserModel2>(true);
				
				var data = new List<UserModel2>() {
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em1@acme.org" },
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em2@acme.org" },
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em3@acme.org" },
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em4@acme.org" },
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em5@acme.org" },
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em6@acme.org" },
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em7@acme.org" },
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em8@acme.org" },
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em9@acme.org" },
					new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em0@acme.org" }
				};
				
				await storageContext.StoreAsync(nStoreOperation.mergeOrInserOperation, data);
				            	       				
				var items = (await storageContext.QueryAsync<UserModel2>(5)).AsEnumerable();
				Assert.Equal(5, items.Count());
													    									
				var allitems = await storageContext.QueryAsync<UserModel2>();
				Assert.Equal(10, allitems.Count());

				// Clean up 				
				await storageContext.DeleteAsync<UserModel2>(allitems);
				var result = await storageContext.QueryAsync<UserModel2>();
				Assert.Equal(0, result.Count());				
            }						
		}	
	}
}
