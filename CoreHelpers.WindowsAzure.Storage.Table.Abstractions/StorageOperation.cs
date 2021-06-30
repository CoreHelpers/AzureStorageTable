namespace CoreHelpers.WindowsAzure.Storage.Table.Abstractions
{
    public enum StorageOperation
    {
        InsertOperation, 
        InsertOrReplaceOperation,
        MergeOperation,
        MergeOrInserOperation,
        Delete
    }
}