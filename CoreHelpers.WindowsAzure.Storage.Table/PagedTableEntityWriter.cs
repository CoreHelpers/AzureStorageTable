using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	public class PagedTableEntityWriter<T> : IDisposable where T : new()
	{
		private List<T> _modelCache { get; set; } = new List<T>();
		private int _pageSize { get; set; }
		private nStoreOperation _operation { get; set; }
		private StorageContext _parentContext { get; set; }
		
		public PagedTableEntityWriter(StorageContext parentContext, nStoreOperation operation, int pageSize) 
		{
			_pageSize = pageSize > 100 ? 100 : pageSize;
			_operation = operation;
			_parentContext = parentContext;
		}
		
		private async Task SyncModels(List<T> cacheToSync) 
		{
			// store
			using(var context = new StorageContext(_parentContext)) 
			{
				await context.StoreAsync(_operation, cacheToSync);
			}            
		}
		
		public async Task StoreAsync(IEnumerable<T> models) 
		{
			var cacheToSync = default(List<T>);

			lock (_modelCache)
			{
				// add to the list 
				_modelCache.AddRange(models);

				// check if we are above the page size 
				if (_modelCache.Count >= _pageSize)
				{
					cacheToSync = new List<T>(_modelCache);
					_modelCache.Clear();
				}
			}
                        
			// sync
			if (cacheToSync != null)
				await SyncModels(cacheToSync);				
		}
		
		public async Task StoreAsync(T model) 
		{
			await StoreAsync(new List<T>() { model });
		}

		public void Dispose()
		{
			lock (_modelCache)
			{
				if (_modelCache.Count > 0)
				{
					// sync
					SyncModels(new List<T>(_modelCache)).ConfigureAwait(false).GetAwaiter().GetResult();
				}
			}
		}
	}
}
