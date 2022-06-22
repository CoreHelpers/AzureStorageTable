using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC10CreateHugeAmountOfDemoEntries : IDemoCase
	{
		public async Task Execute(string connectionString)
		{
			Console.WriteLine("");
			Console.WriteLine(this.GetType().FullName);
			
			using (var storageContext = new StorageContext(connectionString))
			{
				// set our delegate 
				var stats = new DemoCaseStatsDelegate();
				storageContext.SetDelegate(stats);
				
				// ensure we are using the attributes
				storageContext.AddAttributeMapper(typeof(HugeDemoEntry));

				// create 2000 items
				var data = new List<HugeDemoEntry>();
				for (int i = 0; i < 2000; i++)
					data.Add(new HugeDemoEntry());

				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<HugeDemoEntry>(data);
				
				// query all entries
				var items = await storageContext.QueryAsync<HugeDemoEntry>();

				// remove all entries
				await storageContext.DeleteAsync<HugeDemoEntry>(items);

				// dump stats				
				stats.DumpStats();
			}
		}
	}	
}
