using System;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC06AutoCreateTable : IDemoCase
	{
		public async Task Execute(string storageKey, string storageSecret, string endpointSuffix = null)
		{			
			Console.WriteLine("");
			Console.WriteLine(this.GetType().FullName);

            // auto create
            await AutoCreateDuringStore(storageKey, storageSecret, endpointSuffix);

            // read
            await AutoCreateDuringRead(storageKey, storageSecret, endpointSuffix);
		}

        private async Task AutoCreateDuringStore(string storageKey, string storageSecret, string endpointSuffix = null) {
            
            using (var storageContext = new StorageContext(storageKey, storageSecret, endpointSuffix))
            {     
                // set the delegate
                var stats = new DemoCaseStatsDelegate();
                storageContext.SetDelegate(stats);
                 
                // create a new user
                var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
                
                // generate tablename
                Console.WriteLine("Generating Tablename");
                var tableName = "T" + Guid.NewGuid().ToString();
                tableName = tableName.Replace("-", "");
                                
                // ensure we are using the attributes
                Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = tableName, PartitionKeyFormat = "Contact", RowKeyFormat = "Contact" });
                                
                // inser the model
                Console.WriteLine("Insert Models with auto table creation");
                await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<UserModel>(user);                
        
                // query all
                Console.WriteLine("Query all Models");
                var result = await storageContext.QueryAsync<UserModel>();
        
                foreach (var r in result)
                {
                    Console.WriteLine(r.LastName);                  
                }
                
                // Clean up 
                Console.WriteLine("Removing all entries");          
                await storageContext.DeleteAsync<UserModel>(result);                                                
                
                // dump the stats 
                stats.DumpStats();                        
            }                       
        }

        private async Task AutoCreateDuringRead(string storageKey, string storageSecret, string endpointSuffix = null)
        { 
            using (var storageContext = new StorageContext(storageKey, storageSecret, endpointSuffix))
            {                
                // create a new user
                var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };

                // generate tablename
                Console.WriteLine("Generating Tablename");
                var tableName = "T" + Guid.NewGuid().ToString();
                tableName = tableName.Replace("-", "");

                // ensure we are using the attributes
                Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = tableName, PartitionKeyFormat = "Contact", RowKeyFormat = "Contact" });

                // query all
                Console.WriteLine("Query all Models");
                var result = await storageContext.EnableAutoCreateTable().QueryAsync<UserModel>();
                Console.WriteLine($"Result is {result.Count()}");
            }
        }
	}
}
