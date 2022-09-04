using System;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Backup.Abstractions
{
    public interface IBackupContext : IDisposable
    {
        Task Backup(IStorageContext storageContext, string[] excludedTables = null, bool compress = true);
    }
}

