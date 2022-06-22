using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC08CheckMaxItems : IDemoCase
	{
		public async Task Execute(string connectionString)
		{			
			Console.WriteLine("");
			Console.WriteLine(this.GetType().FullName);
						
            using (var storageContext = new StorageContext(connectionString))
            {     
        		// set the delegate
				var stats = new DemoCaseStatsDelegate();
				storageContext.SetDelegate(stats);
			     	
		     	Console.WriteLine("Configuring Entity Mappers");
				storageContext.AddAttributeMapper(typeof(UserModel2), "DemoUserModel2");
				
				Console.WriteLine("Create Tables");
				storageContext.CreateTable<UserModel2>(true);

				Console.WriteLine("InsertData");
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
				
            	        
				Console.WriteLine("Query max Item Models");
				var items = (await storageContext.QueryAsync<UserModel2>(5)).AsEnumerable();
				Console.WriteLine("Found {0} items", items.Count());
				if (items.Count() != 5)
					Console.WriteLine("OHOH should be 5");					
							    						
				Console.WriteLine("Query all items");
				var allitems = await storageContext.QueryAsync<UserModel2>();
				
                // Clean up 
				Console.WriteLine("Removing all entries");			
				await storageContext.DeleteAsync<UserModel2>(allitems);												
				
				// dump the stats 
				stats.DumpStats();                        
            }						
		}	
	}
}
