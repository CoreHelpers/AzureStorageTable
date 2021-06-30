using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Abstractions
{
    public interface IStorageContextQueryCursor<out T> : IDisposable where T : new()
    {
        IQueryable<T> Items { get; }

        int Page { get; }

        Task<bool> LoadNextPageAsync();
    }
}