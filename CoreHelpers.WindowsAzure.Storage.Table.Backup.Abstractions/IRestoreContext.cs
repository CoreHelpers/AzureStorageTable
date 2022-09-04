using System;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Backup.Abstractions
{
    public interface IRestoreContext : IDisposable
    {
        // Task BackupTable(IStorageContext storageContext, string tableName, bool compress = true);
    }
}

