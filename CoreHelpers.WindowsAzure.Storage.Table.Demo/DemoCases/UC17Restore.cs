using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
    public class UC17Restore : IDemoCase
    {
        public async Task Execute(string connectionString)
        {
            // Import from Blob
            using (var storageContext = new StorageContext(connectionString))
            {
                await Task.CompletedTask;

                /*var backupStorage = new CloudStorageAccount(new StorageCredentials(storageKey, storageSecret), endpointSuffix, true);

                var backupService = new BackupService(storageContext, backupStorage, new DemoStorageLogger());

                await backupService.Restore("DemoBck", "bck01", "R1");*/
            }
        }
    }
}
