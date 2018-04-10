using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table;
using System.IO;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
    public class UC16Backup : IDemoCase
    {        
        public async Task Execute(string storageKey, string storageSecret, string endpointSuffix = null)
        {
            Console.WriteLine("");
            Console.WriteLine(this.GetType().FullName);

            using (var storageContext = new StorageContext(storageKey, storageSecret, endpointSuffix))
            {
                using(var textWriter = new StreamWriter("/tmp/test.csv")) {
                    await storageContext.Export("ExportDemo", textWriter, null);    
                }

            }
        }
    }
}
