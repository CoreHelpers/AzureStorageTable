using System;
using System.Collections.Generic;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public enum nStoreOperation
    {
        insertOperation,
        insertOrReplaceOperation,
        mergeOperation,
        mergeOrInserOperation,
        delete
    }

    public interface IStorageContextDelegate
	{
		void OnQuerying(Type modelType, string filter, int maxItems, bool isContinuationQuery);

		void OnQueryed(Type modelType, string filter, int maxItems, bool isContinuationQuery, Exception e);

		void OnStoring(Type modelType, nStoreOperation storaeOperationType);

		void OnStored(Type modelType, nStoreOperation storaeOperationType, int modelCount, Exception e);
	}
}
