using System;
using System.Collections.Generic;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	public interface IStorageContextDelegate
	{
		void OnQuerying(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery);

		void OnQueryed(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery, Exception e);

		void OnStoring(Type modelType, nStoreOperation storaeOperationType);

		void OnStored(Type modelType, nStoreOperation storaeOperationType, int modelCount, Exception e);
	}
}
