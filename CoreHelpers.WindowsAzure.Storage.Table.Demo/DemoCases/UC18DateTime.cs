using System;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
    public class UC18DateTime : IDemoCase
    {
        public async Task Execute(string storageKey, string storageSecret, string endpointSuffix = null)
        {
            // Import from Blob
            using (var storageContext = new StorageContext(storageKey, storageSecret, endpointSuffix))
            {
                // create the model 
                var model = new DatetimeModel() { ActivatedAt = DateTime.Now.ToUniversalTime() };

                // save the time
                var dt = model.ActivatedAt;

                // ensure we are using the attributes
                Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddAttributeMapper(typeof(DatetimeModel));

                // ensure the table exists
                Console.WriteLine("Create Tables");
                await storageContext.CreateTableAsync<DatetimeModel>();

                // inser the model
                Console.WriteLine("Insert Models");
                await storageContext.MergeOrInsertAsync<DatetimeModel>(model);

                // query all
                Console.WriteLine("Query all Models");
                var result = await storageContext.QueryAsync<DatetimeModel>();

                // get the first
                if (result.First().ActivatedAt != dt)
                    Console.WriteLine("Oh NO");

                // Clean up 
                Console.WriteLine("Removing all entries");
                await storageContext.DeleteAsync<DatetimeModel>(result);
            }
        }
    }
}
