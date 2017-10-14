using System;
using CoreHelpers.WindowsAzure.Storage.Table.Delegates;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers
{
	public class DemoCaseStatsDelegate : StorageContextStatsDelegate
	{		
		public void DumpStats() 
		{
			Console.WriteLine("");
			Console.WriteLine("Executed Write Operations:");

			foreach(var kvp in StoreOperations) {
				Console.WriteLine("\t{0}: {1}", kvp.Key.ToString(), kvp.Value);
			}
			
			Console.WriteLine("Executed Query Operations:");
				Console.WriteLine("\tQuery: {0}", QueryOperations);
		}
	}
}
