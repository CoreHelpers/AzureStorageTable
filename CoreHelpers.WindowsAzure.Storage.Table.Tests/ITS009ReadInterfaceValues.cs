using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using Xunit.DependencyInjection;
using CoreHelpers.WindowsAzure.Storage.Table.Tests;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
	[Startup(typeof(Startup))]
	[Collection("Sequential")]
	public class ITS009ReadInterfaceValues
	{
		private readonly ITestEnvironment env;

		public ITS009ReadInterfaceValues(ITestEnvironment env)
		{
			this.env = env;
		}

		[Fact]
		public async Task VerifyReadingInterfaceValues()
		{			
			using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // set the tablename context
                storageContext.SetTableContext();

                // create a new user								
                var user02 = new UserModel3() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
				user02.Codes.Add(new Code() { CodeType = "x1", CodeValue = "x2" });
				user02.Codes.Add(new Code() { CodeType = "x3", CodeValue = "x4" });
			     			     	
				storageContext.AddAttributeMapper(typeof(UserModel3));
																
				await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<UserModel3>(user02);
				            	
				var result = await storageContext.QueryAsync<UserModel3>();
				Assert.Single(result);
				Assert.Equal("Mueller", result.First().LastName);
				Assert.Equal(2, result.Last().Codes.Count());
				Assert.Equal("x1", result.Last().Codes.First().CodeType);
				
                // Clean up 
				await storageContext.DeleteAsync<UserModel3>(result);
				result = await storageContext.QueryAsync<UserModel3>();
				Assert.Empty(result);

                await storageContext.DropTableAsync<UserModel3>();
            }						
		}	
	}
}
