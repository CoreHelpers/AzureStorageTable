using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CoreHelpers.WindowsAzure.Storage.Table
{	
	public class StorageContextQueryCursor<T> : IDisposable where T : new()
	{
		private StorageContext StorageContext { get; set; }		
		private string PartitionKey { get; set; }
		private string RowKey { get; set; } 
		private int MaxItems { get; set; }
		
		private QueryResult<T> CurrentQueryResult { get; set; }
		
		public IQueryable<T> Items { get { return CurrentQueryResult == null ? (new List<T>()).AsQueryable() : CurrentQueryResult.Items; } }
		public int Page { get; private set; }
		
		public StorageContextQueryCursor(StorageContext parentContext, string partitionKey, string rowKey, int maxItems = 0) 
		{
			StorageContext = new StorageContext(parentContext);
			PartitionKey = partitionKey;
			RowKey = rowKey;
			MaxItems = maxItems;
			Page = 0;			
		}

		public async Task<Boolean> LoadNextPageAsync() 
		{
			// check if we have a query result with no next token 
			if (CurrentQueryResult != null && CurrentQueryResult.NextToken == null)
				return false;
				
			// load the page
			CurrentQueryResult = await StorageContext.QueryAsyncInternalSinglePage<T>(PartitionKey, RowKey, MaxItems, CurrentQueryResult == null ? null : CurrentQueryResult.NextToken);

			// increase page number 
			Page++;
			
			// done
			return true;
		}

		public void Dispose()
		{		
			StorageContext.Dispose();			
		}
	}
}