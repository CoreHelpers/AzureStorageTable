using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	public static class StorageContextSyncExtension
	{
		public static void Insert<T>(this StorageContext context, IEnumerable<T> models) where T : new()
		{
			context.InsertAsync<T>(models).GetAwaiter().GetResult();
		}

		public static void Insert<T>(this StorageContext context, T model) where T : new()
		{
			context.Insert<T>(new List<T>() { model });
		}

		public static void Merge<T>(this StorageContext context, IEnumerable<T> models) where T : new()
		{
			context.MergeAsync<T>(models).GetAwaiter().GetResult();
		}

		public static void Merge<T>(this StorageContext context, T model) where T : new()
		{
			context.MergeAsync<T>(new List<T>() { model }).GetAwaiter().GetResult();
		}

		public static void InsertOrReplace<T>(this StorageContext context, IEnumerable<T> models) where T : new()
		{
			context.InsertOrReplaceAsync<T>(models).GetAwaiter().GetResult();
		}

		public static void InsertOrReplace<T>(this StorageContext context, T model) where T : new()
		{
			context.InsertOrReplaceAsync<T>(new List<T>() { model }).GetAwaiter().GetResult();
		}

		public static void MergeOrInsert<T>(this StorageContext context, IEnumerable<T> models) where T : new() 
		{
			context.MergeOrInsertAsync<T>(models).GetAwaiter().GetResult();
		}

		public static void MergeOrInsert<T>(this StorageContext context, T model) where T : new()
		{
			context.MergeOrInsertAsync<T>(new List<T>() { model }).GetAwaiter().GetResult();
		}

		public static IQueryable<T> Query<T>(this StorageContext context, TableContinuationToken continuationToken = null) where T : new()
		{
			return context.QueryAsync<T>(continuationToken).GetAwaiter().GetResult();
		}

		public static T Query<T>(this StorageContext context, string partitionKey, string rowKey) where T : new()
		{
			return context.QueryAsync<T>(partitionKey, rowKey).GetAwaiter().GetResult();
		}

		public static IQueryable<T> Query<T>(this StorageContext context, string partitionKey, TableContinuationToken continuationToken = null) where T : new() 
		{
			return context.QueryAsync<T>(partitionKey, continuationToken).GetAwaiter().GetResult();	
		}
	}
}
