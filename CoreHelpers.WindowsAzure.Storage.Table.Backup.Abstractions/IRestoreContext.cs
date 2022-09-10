using System;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Backup.Abstractions
{
    public interface IRestoreContext : IDisposable
    {
        Task Restore(IStorageContext storageContext, string[] excludedTables = null);
    }
}

