using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CoreHelpers.WindowsAzure.Storage.Table.Backup.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoreHelpers.WindowsAzure.Storage.Table.Backup
{
    public class RestoreContext : IRestoreContext
    {
        private ILogger<RestoreContext> _logger;
        private BlobServiceClient _blobServiceClient;

        private string _sourceConnectionString;
        private string _sourceContainer;
        private string _sourcePath;
        private string _sourceTableNamePrefix;

        public RestoreContext(ILogger<RestoreContext> logger, string connectionString, string container, string path, string tableNamePrefix)
        {
            _logger = logger;
            _blobServiceClient = new BlobServiceClient(connectionString);

            _sourceConnectionString = connectionString;
            _sourceContainer = container;
            _sourcePath = path;
            _sourceTableNamePrefix = tableNamePrefix;
        }

        public void Dispose()
        {            
        }

        public async Task Restore(IStorageContext storageContext, string[] excludedTables = null)
        {
            using (_logger.BeginScope("Starting restore procedure..."))
            {
                // get the container reference
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_sourceContainer);
                if (!await blobContainerClient.ExistsAsync())
                    throw new Exception("Container not found");

                // check if the container exists                
                if (!await blobContainerClient.ExistsAsync())
                {
                    _logger.LogInformation($"Missing container {_sourceContainer.ToLower()}");
                    return;
                }

                // build the path including prefix 
                _logger.LogInformation($"Search Prefix is {_sourcePath}");

                // get the pages
                var blobPages = blobContainerClient.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.None, Azure.Storage.Blobs.Models.BlobStates.None, _sourcePath).AsPages();

                // visit every page
                await foreach (var page in blobPages)
                {
                    foreach(var blob in page.Values)
                    {
                        // build the name 
                        var blobName = blob.Name;
                        
                        // get the tablename 
                        var tableName = Path.GetFileNameWithoutExtension(blobName);
                        var compressed = blobName.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase);
                        if (compressed)
                            tableName = Path.GetFileNameWithoutExtension(tableName);

                        // add the prefix
                        if (!String.IsNullOrEmpty(_sourceTableNamePrefix))
                            tableName = $"{_sourceTableNamePrefix}{tableName}";

                        // log
                        _logger.LogInformation($"Restoring {blobName} to table {tableName} (Compressed: {compressed})");

                        // open the read stream
                        var blobClient = blobContainerClient.GetBlobClient(blob.Name);
                        using (var readStream = await blobClient.OpenReadAsync())
                        {
                            // unzip the stream 
                            using (var contentReader = new ZippedStreamReader(readStream, compressed))
                            {
                                // import the stream
                                var pageCounter = 0;
                                await storageContext.ImportFromJsonAsync(tableName, contentReader, (c) =>
                                {
                                    switch (c)
                                    {
                                        case ImportExportOperation.processingPage:
                                            _logger.LogInformation($"  Processing page #{pageCounter}...");
                                            break;
                                        case ImportExportOperation.processedPage:
                                            pageCounter++;
                                            break;
                                    }                                                                        
                                });
                            }
                        }
                    }
                }              
            }

            await Task.CompletedTask;
        }
    }
}