using System;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Abstractions;

namespace CoreHelpers.WindowsAzure.Storage.Table.Delegates
{
	public class StorageContextStatsDelegate : IStorageContextDelegate
	{
		protected Dictionary<StorageOperation, int> StoreOperations { get; set; } = new Dictionary<StorageOperation, int>();
		protected int QueryOperations { get; set; }
	  	
	    public void OnQueried(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery, Exception e)
	    {
		    QueryOperations += 1;	
	    }
	    
	    [Obsolete(nameof(OnQueryed) + " is deprecated, use " + nameof(OnQueried) + " instead.")]
	    public void OnQueryed(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery, Exception e)
	    {
		    QueryOperations += 1;	
	    }

		public void OnQuerying(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery)
		{
			
		}

		public void OnStored(Type modelType, StorageOperation storaeOperationType, int modelCount, Exception e)
		{
			if (!StoreOperations.ContainsKey(storaeOperationType))
				StoreOperations[storaeOperationType] = 0;
			StoreOperations[storaeOperationType]++;
		}

		public void OnStoring(Type modelType, StorageOperation storaeOperationType)
		{
			
		}				
	}
}
