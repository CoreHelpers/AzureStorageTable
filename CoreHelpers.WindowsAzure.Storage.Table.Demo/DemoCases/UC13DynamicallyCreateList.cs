using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
    public class UC13DynamicallyCreateList : IDemoCase
    {
        public async Task Execute(string connectionString)
        {
            Console.WriteLine("");
            Console.WriteLine(this.GetType().FullName);

            using (var storageContext = new StorageContext(connectionString))
            {
                // create model with data in list
                var model = new DemoMeterModel() { ExtendedCosts = new List<Double>() };
                model.ExtendedCosts.Add(5.5);
                model.ExtendedCosts.Add(6.0);

                var model2 = new DemoMeterModel() { R = "R2" };

                // ensure we are using the attributes
                Console.WriteLine("Configuring Entity Mappers");
                storageContext.AddAttributeMapper(typeof(DemoMeterModel));

                // ensure the table exists
                Console.WriteLine("Create Tables");
                await storageContext.CreateTableAsync<DemoMeterModel>();
        
                // inser the model
                Console.WriteLine("Insert Models");
                await storageContext.MergeOrInsertAsync<DemoMeterModel>(model);
                await storageContext.MergeOrInsertAsync<DemoMeterModel>(model2);
        
                // query all
                Console.WriteLine("Query all Models");
                var result = await storageContext.QueryAsync<DemoMeterModel>();

                foreach (var item in result)
                {
                    if (item.ExtendedCosts != null)
                    {
                        foreach (var dc in item.ExtendedCosts)
                        {
                            Console.Write(dc);
                        }

                        Console.WriteLine("");
                    } else { 
                        Console.WriteLine("No Extended Costs");
                    }
                }

                // Clean up 
                Console.WriteLine("Removing all entries");
                await storageContext.DeleteAsync<DemoMeterModel>(result);
            }       
        }
    }
}
