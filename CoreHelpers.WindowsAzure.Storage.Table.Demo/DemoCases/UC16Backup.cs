using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
    internal class DemoStorageLogger : DefaultStorageLogger
    {
        public override void LogInformation(string text)
        {
            Console.WriteLine(text);
        }
    }

    public class UC16Backup : IDemoCase
    {
        public async Task Execute(string connectionString)
        {
            Console.WriteLine("");
            Console.WriteLine(this.GetType().FullName);

            // Export Table
            using (var storageContext = new StorageContext(connectionString))
            {
                using (var textWriter = new StreamWriter("/tmp/test.json"))
                {
                    await storageContext.ExportToJsonAsync("ExportDemo", textWriter);
                }

            }

            // Export to Blob
            using (var storageContext = new StorageContext(connectionString))
            {
                /*var backupStorage = new CloudStorageAccount(new StorageCredentials(storageKey, storageSecret), endpointSuffix, true);

                var backupService = new BackupService(storageContext, backupStorage, new DemoStorageLogger());

                await backupService.Backup("DemoBck", "bck01");*/
            }
        }
    }
}
