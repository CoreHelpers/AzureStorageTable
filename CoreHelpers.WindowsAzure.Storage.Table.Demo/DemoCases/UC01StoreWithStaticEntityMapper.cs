using System;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC01StoreWithStaticEntityMapper : IDemoCase
	{
		public async Task Execute(string connectionString)
		{			
			Console.WriteLine("");
			Console.WriteLine(this.GetType().FullName);
			
			using (var storageContextParent = new StorageContext(connectionString))
			{
				// set the delegate
				var stats = new DemoCaseStatsDelegate();
				storageContextParent.SetDelegate(stats);
								
				// create a new user
				var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
				user.Contact = user.Contact + Guid.NewGuid().ToString();

				var vpmodel = new VirtualPartitionKeyDemoModelPOCO() { Value1 = "abc", Value2 = "def", Value3 = "ghi" };

				// configure the entity mapper
				Console.WriteLine("Configuring Entity Mappers");
				storageContextParent.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = "UserProfiles", PartitionKeyFormat = "Contact", RowKeyFormat = "Contact" });
				storageContextParent.AddEntityMapper(typeof(VirtualPartitionKeyDemoModelPOCO), new DynamicTableEntityMapper() { TableName = "VirtualPartitionKeyDemoModelPOCO", PartitionKeyFormat = "{{Value1}}-{{Value2}}", RowKeyFormat = "{{Value2}}-{{Value3}}" });

				using (var storageContext = new StorageContext(storageContextParent))
				{					
					// ensure the table exists
					Console.WriteLine("Create Tables");
					await storageContext.CreateTableAsync<UserModel>();
					await storageContext.CreateTableAsync<VirtualPartitionKeyDemoModelPOCO>();

					// inser the model
					Console.WriteLine("Insert Models");
					await storageContext.MergeOrInsertAsync<UserModel>(user);
					await storageContext.MergeOrInsertAsync<VirtualPartitionKeyDemoModelPOCO>(vpmodel);
				}

				// query all
				Console.WriteLine("Query all Models");
				var result = await storageContextParent.QueryAsync<UserModel>();
				var resultVP = await storageContextParent.QueryAsync<VirtualPartitionKeyDemoModelPOCO>();

				foreach (var r in result)				
					Console.WriteLine(r.FirstName);
				
				// Clean up 
				Console.WriteLine("Removing all entries");			
				await storageContextParent.DeleteAsync<UserModel>(result);
				await storageContextParent.DeleteAsync<VirtualPartitionKeyDemoModelPOCO>(resultVP);

				// dump the stats 
				stats.DumpStats();
			}
		}	
	}
}
