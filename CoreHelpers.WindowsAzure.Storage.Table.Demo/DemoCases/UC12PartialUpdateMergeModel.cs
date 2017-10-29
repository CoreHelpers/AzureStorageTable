using System;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
    public class UC12PartialUpdateMergeModel : IDemoCase
    {
        public async Task Execute(string storageKey, string storageSecret, string endpointSuffix = null)
        {
            Console.WriteLine("");
            Console.WriteLine(this.GetType().FullName);

            using (var storageContext = new StorageContext(storageKey, storageSecret, endpointSuffix))
            {                     
                // create a new user
                var model = new DemoEntryWithOptionalValues() { Identifier = "X" };            
        
                // ensure we are using the attributes
                Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddAttributeMapper();
                
                // ensure the table exists
                Console.WriteLine("Create Tables");
                await storageContext.CreateTableAsync<DemoEntryWithOptionalValues>();
        
                // inser the model
                Console.WriteLine("Insert Models");
                await storageContext.MergeOrInsertAsync<DemoEntryWithOptionalValues>(model);
        
                // query all
                Console.WriteLine("Query all Models");
                var result = (await storageContext.QueryAsync<DemoEntryWithOptionalValues>()).FirstOrDefault();

                // check 
                if (result.Name != null)
                    Console.WriteLine("OhOh should be null");

                if (result.Costs.HasValue)
                    Console.WriteLine("OhOh should have no value");


                // update the model
                Console.WriteLine("Insert Models");
                result.Costs = 5.4;
                await storageContext.MergeOrInsertAsync<DemoEntryWithOptionalValues>(result);
                                       
                // query all
                Console.WriteLine("Query all Models");
                result = (await storageContext.QueryAsync<DemoEntryWithOptionalValues>()).FirstOrDefault();

                // check 
                if (result.Name != null)
                    Console.WriteLine("OhOh should be null");

                if (!result.Costs.HasValue)
                    Console.WriteLine("OhOh should have a value");
                else
                    Console.WriteLine($"Value: {result.Costs.Value}");
                
                // Clean up 
                Console.WriteLine("Removing all entries");          
                await storageContext.DeleteAsync<DemoEntryWithOptionalValues>(result);
            }       
        }
    }
}
