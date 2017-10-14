using System;
using System.Collections.Generic;

namespace CoreHelpers.WindowsAzure.Storage.Table.Delegates
{
	public class StorageContextStatsDelegate : IStorageContextDelegate
	{
		public Dictionary<nStoreOperation, int> StoreOperations { get; set; } = new Dictionary<nStoreOperation, int>();
	  	public int QueryOperations { get; set; }
	  	
		public void OnQueryed(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery, Exception e)
		{
			QueryOperations += 1;	
		}

		public void OnQuerying(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery)
		{
			
		}

		public void OnStored(Type modelType, nStoreOperation storaeOperationType, int modelCount, Exception e)
		{
			if (!StoreOperations.ContainsKey(storaeOperationType))
				StoreOperations[storaeOperationType] = 0;
			StoreOperations[storaeOperationType]++;
		}

		public void OnStoring(Type modelType, nStoreOperation storaeOperationType)
		{
			
		}				
	}
}
