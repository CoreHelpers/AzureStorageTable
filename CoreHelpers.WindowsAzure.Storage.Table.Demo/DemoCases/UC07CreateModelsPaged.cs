using System;
using System.Threading.Tasks;
using System.Linq;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC07CreateModelsPaged : IDemoCase
	{
		public async Task Execute(string storageKey, string storageSecret, string endpointSuffix = null)
		{			
			Console.WriteLine("");
			Console.WriteLine(this.GetType().FullName);
						
            using (var storageContext = new StorageContext(storageKey, storageSecret, endpointSuffix))
            {     
        		// set the delegate
				var stats = new DemoCaseStatsDelegate();
				storageContext.SetDelegate(stats);
			     			    						
				// ensure we are using the attributes
				Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddAttributeMapper(typeof(UserModel2), "DemoUserModel2");
                
                // create tables
                Console.WriteLine("Create Tables");
				await storageContext.CreateTableAsync<UserModel2>(true);

				// write data pages		
				Console.WriteLine("Writing Models Paged");
                var startDate = DateTime.Now;
				
				using (var pagedWriter = new PagedTableEntityWriter<UserModel2>(storageContext, nStoreOperation.insertOrReplaceOperation, 100))
				{					
					for (var i = 0; i < 1000; i++) 
					{
						var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = string.Format("em-{0}@acme.org", i) };
						await pagedWriter.StoreAsync(user);
					}
				}

				var endDate = DateTime.Now;
				
				Console.WriteLine("Took {0} seconds", (endDate- startDate).TotalSeconds);                
        
                // query all
                Console.WriteLine("Query all Models");
                var result = await storageContext.QueryAsync<UserModel2>();
				Console.WriteLine("Found {0} models", result.Count());        
				
                // Clean up 
				Console.WriteLine("Removing all entries");			
				await storageContext.DeleteAsync<UserModel2>(result);												
				
				// dump the stats 
				stats.DumpStats();                        
            }						
		}	
	}
}
