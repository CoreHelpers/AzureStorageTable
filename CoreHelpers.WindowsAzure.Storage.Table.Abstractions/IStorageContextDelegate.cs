using System;

namespace CoreHelpers.WindowsAzure.Storage.Table.Abstractions
{
	public interface IStorageContextDelegate
	{
		void OnQuerying(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery);

		void OnQueried(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery, Exception e);
		
		[Obsolete(nameof(OnQueryed) + " is deprecated, use " + nameof(OnQueried) + " instead.")]
		void OnQueryed(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery, Exception e);

		void OnStoring(Type modelType, StorageOperation storageOperation);

		void OnStored(Type modelType, StorageOperation storageOperation, int modelCount, Exception e);
	}
}
