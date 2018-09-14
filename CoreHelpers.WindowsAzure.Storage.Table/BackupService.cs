using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Services;
using Microsoft.WindowsAzure.Storage;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public class BackupService
    {
        private StorageContext tableStorageContext { get; set; }
        private CloudStorageAccount backupStorageAccount { get; set; }
        private IStorageLogger storageLogger { get; set; }

        private DataExportService dataExportService { get; set; }

        public BackupService(StorageContext tableStorageContext, CloudStorageAccount backupStorageAccount, IStorageLogger storageLogger)
        {
            this.tableStorageContext = tableStorageContext;
            this.backupStorageAccount = backupStorageAccount;
            this.dataExportService = new DataExportService(tableStorageContext);
            this.storageLogger = storageLogger;
        }

        public async Task Backup(string containerName, string targetPath, string tableNamePrefix = null, bool compress = true)
        {
            // log 
            storageLogger.LogInformation($"Starting backup procedure...");

            // get all tables 
            var tables = await tableStorageContext.QueryTableList();
            storageLogger.LogInformation($"Processing {tables.Count} tables");

            // prepare the backup container
            var backupBlobClient = backupStorageAccount.CreateCloudBlobClient();
            var backupContainer = backupBlobClient.GetContainerReference(containerName.ToLower());
            storageLogger.LogInformation($"Creating target container {containerName} if needed");
            await backupContainer.CreateIfNotExistsAsync();

            // visit every table
            foreach (var tableName in tables)
            {
                // filter the table prefix
                if (!String.IsNullOrEmpty(tableNamePrefix) && !tableName.StartsWith(tableNamePrefix, StringComparison.CurrentCulture))
                {
                    storageLogger.LogInformation($"Ignoring table {tableName}...");
                    continue;
                } else {
                    storageLogger.LogInformation($"Processing table {tableName}...");
                }

                // do the backup
                var fileName = $"{tableName}.json";
                if (!string.IsNullOrEmpty(targetPath)) { fileName = $"{targetPath}/{fileName}"; }
                if (compress) { fileName += ".gz"; }

                // open block blog reference
                var blockBlob = backupContainer.GetBlockBlobReference(fileName);

                using (var backupFileStream = await blockBlob.OpenWriteAsync())
                {
                    if (compress)
                    {
                        using (var compressionStream = new GZipStream(backupFileStream, CompressionMode.Compress))
                        {
                            using (var contentWriter = new StreamWriter(compressionStream))
                            {
                                var pageCounter = 0;
                                await dataExportService.ExportToJson(tableName, contentWriter, (c) => {
                                    pageCounter++;
                                    storageLogger.LogInformation($"  Processing page #{pageCounter} with #{c} items...");
                                });
                            }
                        }
                    }
                    else
                    {
                        using (var contentWriter = new StreamWriter(backupFileStream))
                        {
                            var pageCounter = 0;
                            await dataExportService.ExportToJson(tableName, contentWriter, (c) => {
                                pageCounter++;
                                storageLogger.LogInformation($"  Processing page #{pageCounter} with #{c} items...");
                            });
                        }
                    }
                }
            }
        }
    }
}