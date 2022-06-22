using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC05StoreAsJson : IDemoCase
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
			     
			    // create the model 
			    var model = new JObjectModel() { UUID = "112233" };
				model.Data.Add("HEllo", "world");
				model.Data2.Value = "Hello 23";
				
				// ensure we are using the attributes
				Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddAttributeMapper(typeof(JObjectModel));                
                
                // ensure the table exists
                Console.WriteLine("Create Tables");
                await storageContext.CreateTableAsync<JObjectModel>();                
        
                // inser the model
                Console.WriteLine("Insert Models");
                await storageContext.MergeOrInsertAsync<JObjectModel>(model);                
        
                // query all
                Console.WriteLine("Query all Models");
                var result = await storageContext.QueryAsync<JObjectModel>();
        
                foreach (var r in result)
                {
                    Console.WriteLine(r.UUID);

					foreach (var e in r.Data)
						Console.WriteLine(e.Key + "-" + e.Value);
                }
                
                // Clean up 
				Console.WriteLine("Removing all entries");			
				await storageContext.DeleteAsync<JObjectModel>(result);				
				
				// dump the stats 
				stats.DumpStats();                        
            }						
		}	
	}
}
