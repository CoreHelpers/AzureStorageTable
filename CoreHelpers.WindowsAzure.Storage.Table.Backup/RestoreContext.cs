using System;
using CoreHelpers.WindowsAzure.Storage.Table.Backup.Abstractions;

namespace CoreHelpers.WindowsAzure.Storage.Table.Backup
{
    public class RestoreContext : IRestoreContext
    {
        public RestoreContext()
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

/*
 * public async Task Restore(string containerName, string srcPath, string tablePrefix = null) {

            // log 
            storageLogger.LogInformation($"Starting restore procedure...");

            // get all backup files 
            var blobClient = backupStorageAccount.CreateCloudBlobClient();
            var containerReference = blobClient.GetContainerReference(containerName.ToLower());

            // check if the container exists
            if (!await containerReference.ExistsAsync()) {
                storageLogger.LogInformation($"Missing container {containerName.ToLower()}");
                return;
            }

            // build the path including prefix 
            storageLogger.LogInformation($"Search Prefix is {srcPath}");

            // track the state
            var continuationToken = default(BlobContinuationToken);

            do
            {
                // get all blobs
                var blobResult = await containerReference.ListBlobsSegmentedAsync(srcPath, true, BlobListingDetails.All, 1000, continuationToken, null, null);

                // process every backup file as table 
                foreach(var blob in blobResult.Results) {

                    // build the name 
                    var blobName = blob.StorageUri.PrimaryUri.AbsolutePath;
                    blobName = blobName.Remove(0, containerName.Length + 2);

                    // get the tablename 
                    var tableName = Path.GetFileNameWithoutExtension(blobName);
                    var compressed = blobName.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase);
                    if (compressed)
                        tableName = Path.GetFileNameWithoutExtension(tableName);

                    // add the prefix
                    if (!String.IsNullOrEmpty(tablePrefix))
                        tableName = $"{tablePrefix}{tableName}";

                    // log
                    storageLogger.LogInformation($"Restoring {blobName} to table {tableName} (Compressed: {compressed})");

                    // build the reference
                    var blockBlobReference = containerReference.GetBlockBlobReference(blobName);

                    // open the read stream 
                    using (var readStream = await blockBlobReference.OpenReadAsync())
                    {
                        // unzip the stream 
                        using (var contentReader = new ZippedStreamReader(readStream, compressed))
                        {
                            // import the stream
                            var pageCounter = 0;
                            await dataImportService.ImportFromJsonStreamAsync(tableName, contentReader, (c) => {
                                pageCounter++;
                                storageLogger.LogInformation($"  Processing page #{pageCounter} with #{c} items...");
                            });
                        }
                    }
                }

                // proces the token 
                continuationToken = blobResult.ContinuationToken;

            } while (continuationToken != null);



        }
*/