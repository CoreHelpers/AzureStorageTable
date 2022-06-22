using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Helpers;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases
{
	public class UC09ReadInterfaceValues : IDemoCase
	{
		public async Task Execute(string connectionString)
		{			
			Console.WriteLine("");
			Console.WriteLine(this.GetType().FullName);
						
            using (var storageContext = new StorageContext(connectionString))
            {     
        		// set the delegate
				var stats = new DemoCaseStatsDelegate();
				storageContext.SetDelegate(stats);

				// create a new user
				Console.WriteLine("Build Models");
				var user01 = new UserModel3() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
				var user02 = new UserModel3() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
				user02.Codes.Add(new Code() { CodeType = "x1", CodeValue = "x2" });
				user02.Codes.Add(new Code() { CodeType = "x3", CodeValue = "x4" });
			     	
		     	Console.WriteLine("Configuring Entity Mappers");
				storageContext.AddAttributeMapper(typeof(UserModel3));
								
				Console.WriteLine("InsertData");
				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<UserModel3>(user01);
				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<UserModel3>(user02);
				
            	Console.WriteLine("Query Data");        
				var result = await storageContext.QueryAsync<UserModel3>();

				foreach (var r in result)
					Console.WriteLine("{0}: {1}", r.LastName, r.Codes.Count());				
				
                // Clean up 
				Console.WriteLine("Removing all entries");			
				await storageContext.DeleteAsync<UserModel3>(result);												
				
				// dump the stats 
				stats.DumpStats();                        
            }						
		}	
	}
}
