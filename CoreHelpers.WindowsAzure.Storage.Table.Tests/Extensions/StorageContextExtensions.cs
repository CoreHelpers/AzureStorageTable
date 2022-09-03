using System;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions
{
    public static class StorageContextExtensions
    {
        public static string BuildTableContext()
        {
            return $"T{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8)}"; 
        }

        public static string SetTableContext(this IStorageContext context)
        {
            var contextValue = BuildTableContext();
            context.SetTableNamePrefix(contextValue);
            return contextValue;
        }
    }
}

