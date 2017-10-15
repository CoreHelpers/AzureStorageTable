using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC11ReadPageByPage : IDemoCase
	{
		public async Task Execute(string storageKey, string storageSecret, string endpointSuffix = null)
		{
			Console.WriteLine("");
			Console.WriteLine(this.GetType().FullName);
			
			using (var storageContext = new StorageContext(storageKey, storageSecret, endpointSuffix))
			{
				// set our delegate 
				var stats = new DemoCaseStatsDelegate();
				storageContext.SetDelegate(stats);
				
				// ensure we are using the attributes
				storageContext.AddAttributeMapper(typeof(HugeDemoEntry));

				// create 4000 items
				Console.WriteLine("Creating 4200 demos items");
				var data = new List<HugeDemoEntry>();
				for (int i = 0; i < 4200; i++)
					data.Add(new HugeDemoEntry());

				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<HugeDemoEntry>(data);
				
				// query items page by page
				Console.WriteLine("Reading page by page");

				var items = new List<HugeDemoEntry>();

				using (var queryCursor = storageContext.QueryPaged<HugeDemoEntry>(null, null))
				{
					while(await queryCursor.LoadNextPageAsync()) 
					{
						Console.WriteLine("Reading Page #{0} with #{1} items", queryCursor.Page, queryCursor.Items.Count());
						items.AddRange(queryCursor.Items);
					}										
				}

				// remove all entries
				Console.WriteLine("Removing all entries");
				await storageContext.DeleteAsync<HugeDemoEntry>(items);

				// dump stats				
				stats.DumpStats();
			}
		}
	}	
}
