using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Backup.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoreHelpers.WindowsAzure.Storage.Table.Backup
{
    public class BackupService : IBackupService
    {        
        private ILoggerFactory _loggerFactory;        

        public BackupService(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;            
        }
        
        public async Task<IBackupContext> OpenBackupContext(string targetBlobStorageConnectionString, string targetContainerName, string targetPath, string tableNamePrefix = null)
        {
            await Task.CompletedTask;
            return new BackupContext(
                _loggerFactory.CreateLogger<BackupContext>(),                
                targetBlobStorageConnectionString, targetContainerName, targetPath,
                tableNamePrefix);
        }

        public async Task<IRestoreContext> OpenRestorContext(string sourceBlobStorageConnectionString, string sourceContainerName, string sourcePath, string tableNamePrefix = null)
        {
            await Task.CompletedTask;
            return new RestoreContext();
        }
    }
}

