using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC04GetVirtualArray : IDemoCase
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
			     
			    // create a virtual array model
            	var model = new VArrayModel() { UUID = "112233" };
				model.DataElements.Add(2);
				model.DataElements.Add(3);
				model.DataElements.Add(4);
				
				// ensure we are using the attributes
				Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddAttributeMapper(typeof(VArrayModel));                
                
                // ensure the table exists
                Console.WriteLine("Create Tables");
                await storageContext.CreateTableAsync<VArrayModel>();                
        
                // inser the model
                Console.WriteLine("Insert Models");
                await storageContext.MergeOrInsertAsync<VArrayModel>(model);                
        
                // query all
                Console.WriteLine("Query all Models");
                var result = await storageContext.QueryAsync<VArrayModel>();
        
                foreach (var r in result)
                {
                    Console.WriteLine(r.UUID);

					foreach (var e in r.DataElements)
						Console.WriteLine(e);
                }
                
                // Clean up 
				Console.WriteLine("Removing all entries");			
				await storageContext.DeleteAsync<VArrayModel>(result);				
				
				// dump the stats 
				stats.DumpStats();                        
            }						
		}	
	}
}
