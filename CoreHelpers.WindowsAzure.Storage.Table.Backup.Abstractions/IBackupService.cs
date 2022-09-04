using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Backup.Abstractions;

namespace CoreHelpers.WindowsAzure.Storage.Table.Backup
{
    public interface IBackupService
    {
        Task<IBackupContext> OpenBackupContext(string targetBlobStorageConnectionString, string targetContainerName, string targetPath, string tableNamePrefix = null);

        Task<IRestoreContext> OpenRestorContext(string sourceBlobStorageConnectionString, string sourceContainerName, string sourcePath, string tableNamePrefix = null);
    }
}

