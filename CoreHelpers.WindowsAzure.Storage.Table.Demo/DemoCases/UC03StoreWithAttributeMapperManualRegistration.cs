using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC03StoreWithAttributeMapperManualRegistration : IDemoCase
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
			     
            	// create a new user            	
            	var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };            
        		var vpmodel = new VirtualPartKeyDemoModel() { Value1 = "abc", Value2 = "def", Value3 = "ghi" };            
        
                // ensure we are using the attributes
                Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddAttributeMapper(typeof(UserModel2));
                storageContext.AddAttributeMapper(typeof(VirtualPartKeyDemoModel));
                
                // ensure the table exists
                Console.WriteLine("Create Tables");
                await storageContext.CreateTableAsync<UserModel2>();
                await storageContext.CreateTableAsync<VirtualPartKeyDemoModel>();                
        
                // inser the model
                Console.WriteLine("Insert Models");
                await storageContext.MergeOrInsertAsync<UserModel2>(user);
                await storageContext.MergeOrInsertAsync<VirtualPartKeyDemoModel>(vpmodel);
        
                // query all
                Console.WriteLine("Query all Models");
                var result = await storageContext.QueryAsync<UserModel2>();
                var resultVP = await storageContext.QueryAsync<VirtualPartKeyDemoModel>();
        
                foreach (var r in result)                
                    Console.WriteLine(r.FirstName);
                
                // Clean up 
				Console.WriteLine("Removing all entries");			
				await storageContext.DeleteAsync<UserModel2>(result);
				await storageContext.DeleteAsync<VirtualPartKeyDemoModel>(resultVP);
				
				// dump the stats 
				stats.DumpStats();
            }						
		}	
	}
}
