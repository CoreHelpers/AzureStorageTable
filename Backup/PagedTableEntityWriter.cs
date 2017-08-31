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
		
		private async Task SyncModels() 
		{
			// store
			using(var context = new StorageContext(_parentContext)) 
			{
				await context.StoreAsync(_operation, _modelCache);
			}

			// clear the cache
			_modelCache.Clear();
		}
		
		public async Task StoreAsync(IEnumerable<T> models) 
		{			
			// add to the list 
			_modelCache.AddRange(models);

			// check if we are above the page size 
			if (_modelCache.Count >= _pageSize) 
			{
				// sync
				await SyncModels();				
			}
		}
		
		public async Task StoreAsync(T model) 
		{
			await StoreAsync(new List<T>() { model });
		}

		public void Dispose()
		{
			if (_modelCache.Count > 0) 
			{
				// sync
				SyncModels().ConfigureAwait(false).GetAwaiter().GetResult();				
			}
		}
	}
}
