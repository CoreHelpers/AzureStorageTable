using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // read the config
            var configLocation = Path.Combine("..", "Credentials.json");
            JObject config = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(configLocation));        

       		StorageWithStaticEntityMapper(config.GetValue("key").ToString(),config.GetValue("secret").ToString());
            StorageWithAttributeMapper(config.GetValue("key").ToString(),config.GetValue("secret").ToString());
            StorageWithAttributeMapperManualRegistration(config.GetValue("key").ToString(),config.GetValue("secret").ToString());            
            GetVirtualArray(config.GetValue("key").ToString(),config.GetValue("secret").ToString());
            StoreAsJson(config.GetValue("key").ToString(),config.GetValue("secret").ToString());
			TestAutoCreateTable(config.GetValue("key").ToString(),config.GetValue("secret").ToString());
			TestAutoCreateTableGermanCloud(config.GetValue("keyde").ToString(), config.GetValue("secretde").ToString(), "core.cloudapi.de");
			CreateModelsPaged(config.GetValue("key").ToString(), config.GetValue("secret").ToString());			
			CheckMaxItems(config.GetValue("key").ToString(), config.GetValue("secret").ToString());
			TestReadInterfaceValues(config.GetValue("key").ToString(), config.GetValue("secret").ToString());
        }

		static void StorageWithStaticEntityMapper(string storageKey, string storageSecret) 
		{
			// create a new user
			var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
			user.Contact = user.Contact + Guid.NewGuid().ToString();

			var vpmodel = new VirtualPartitionKeyDemoModelPOCO() { Value1 = "abc", Value2 = "def", Value3 = "ghi" };
			
			using (var storageContextParent = new StorageContext(storageKey, storageSecret))
			{
				// configure the entity mapper
				storageContextParent.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = "UserProfiles", PartitionKeyFormat = "Contact", RowKeyFormat = "Contact" });
				storageContextParent.AddEntityMapper(typeof(VirtualPartitionKeyDemoModelPOCO), new DynamicTableEntityMapper() { TableName = "VirtualPartitionKeyDemoModelPOCO", PartitionKeyFormat = "{{Value1}}-{{Value2}}", RowKeyFormat = "{{Value2}}-{{Value3}}" });

				using (var storageContext = new StorageContext(storageContextParent))
				{
					// ensure the table exists
					storageContext.CreateTable<UserModel>();
					storageContext.CreateTable<VirtualPartitionKeyDemoModelPOCO>();

					// inser the model
					storageContext.MergeOrInsert<UserModel>(user);
					storageContext.MergeOrInsert<VirtualPartitionKeyDemoModelPOCO>(vpmodel);
				}

				// query all
				var result = storageContextParent.Query<UserModel>();

				foreach (var r in result)
				{
					Console.WriteLine(r.FirstName);
				}				
			}
		}
        
        static void StorageWithAttributeMapper(string storageKey, string storageSecret) 
        {
            // create a new user
            var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };            
        
            using (var storageContext = new StorageContext(storageKey, storageSecret))
            {
                // ensure we are using the attributes
                storageContext.AddAttributeMapper();
                
                // ensure the table exists
                storageContext.CreateTable<UserModel2>();
        
                // inser the model
                storageContext.MergeOrInsert<UserModel2>(user);
        
                // query all
                var result = storageContext.Query<UserModel2>();
        
                foreach (var r in result)
                {
                    Console.WriteLine(r.FirstName);
                }
            }
        }
        
        static void StorageWithAttributeMapperManualRegistration(string storageKey, string storageSecret) 
        {
            // create a new user
            var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };            
        	var vpmodel = new VirtualPartKeyDemoModel() { Value1 = "abc", Value2 = "def", Value3 = "ghi" };
        
            using (var storageContext = new StorageContext(storageKey, storageSecret))
            {
                // ensure we are using the attributes
                storageContext.AddAttributeMapper(typeof(UserModel2));
                storageContext.AddAttributeMapper(typeof(VirtualPartKeyDemoModel));
                
                // ensure the table exists
                storageContext.CreateTable<UserModel2>();
                storageContext.CreateTable<VirtualPartKeyDemoModel>();                
        
                // inser the model
                storageContext.MergeOrInsert<UserModel2>(user);
                storageContext.MergeOrInsert<VirtualPartKeyDemoModel>(vpmodel);
        
                // query all
                var result = storageContext.Query<UserModel2>();
        
                foreach (var r in result)
                {
                    Console.WriteLine(r.FirstName);
                }
            }
        }
        
        static void GetVirtualArray(string storageKey, string storageSecret) 
        {
			var model = new VArrayModel() { UUID = "112233" };
			model.DataElements.Add(2);
			model.DataElements.Add(3);
			model.DataElements.Add(4);
			
			using (var storageContext = new StorageContext(storageKey, storageSecret))
            {
                // ensure we are using the attributes
                storageContext.AddAttributeMapper(typeof(VArrayModel));                
                
                // ensure the table exists
                storageContext.CreateTable<VArrayModel>();                
        
                // inser the model
                storageContext.MergeOrInsert<VArrayModel>(model);                
        
                // query all
                var result = storageContext.Query<VArrayModel>();
        
                foreach (var r in result)
                {
                    Console.WriteLine(r.UUID);

					foreach (var e in r.DataElements)
						Console.WriteLine(e);
                }
            }
        }
        
        static void StoreAsJson(string storageKey, string storageSecret) 
        {
			var model = new JObjectModel() { UUID = "112233" };
			model.Data.Add("HEllo", "world");
			model.Data2.Value = "Hello 23";
			
			using (var storageContext = new StorageContext(storageKey, storageSecret))
            {
                // ensure we are using the attributes
                storageContext.AddAttributeMapper(typeof(JObjectModel));                
                
                // ensure the table exists
                storageContext.CreateTable<JObjectModel>();                
        
                // inser the model
                storageContext.MergeOrInsert<JObjectModel>(model);                
        
                // query all
                var result = storageContext.Query<JObjectModel>();
        
                foreach (var r in result)
                {
                    Console.WriteLine(r.UUID);

					foreach (var e in r.Data)
						Console.WriteLine(e.Key + "-" + e.Value);
                }
            }
        }
        
        static void TestAutoCreateTable(string storageKey, string storageSecret) 
        {		
			// create a new user
			var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
	
			using (var storageContext = new StorageContext(storageKey, storageSecret))
            {
            	// generate tablename
				var tableName = "T" + Guid.NewGuid().ToString();
				tableName = tableName.Replace("-", "");
				
                // ensure we are using the attributes
                storageContext.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = tableName, PartitionKeyFormat = "Contact", RowKeyFormat = "Contact" });
                
                // inser the model
                storageContext.EnableAutoCreateTable().MergeOrInsert<UserModel>(user);                
        
                // query all
                var result = storageContext.Query<UserModel>();
        
                foreach (var r in result)
                {
                    Console.WriteLine(r.LastName);					
                }
            }
        }
        
        static void TestAutoCreateTableGermanCloud(string storageKey, string storageSecret, string storageEndpointSuffix) 
        {		
			// create a new user
			var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
	
			using (var storageContext = new StorageContext(storageKey, storageSecret, storageEndpointSuffix))
            {
            	// generate tablename
				var tableName = "T" + Guid.NewGuid().ToString();
				tableName = tableName.Replace("-", "");
				
                // ensure we are using the attributes
                storageContext.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = tableName, PartitionKeyFormat = "Contact", RowKeyFormat = "Contact" });
                
                // inser the model
                storageContext.EnableAutoCreateTable().MergeOrInsert<UserModel>(user);                
        
                // query all
                var result = storageContext.Query<UserModel>();
        
                foreach (var r in result)
                {
                    Console.WriteLine(r.LastName);					
                }
            }
        }
        
        
        static void CreateModelsPaged(string storageKey, string storageSecret) 
        {
			using (var storageContext = new StorageContext(storageKey, storageSecret))
			{
				storageContext.AddAttributeMapper(typeof(UserModel2), "DemoUserModel2");
				storageContext.CreateTable<UserModel2>(true);

				var startDate = DateTime.Now;
				
				using (var pagedWriter = new PagedTableEntityWriter<UserModel2>(storageContext, nStoreOperation.insertOrReplaceOperation, 100))
				{					
					for (var i = 0; i < 1000; i++) 
					{
						var user = new UserModel2() { FirstName = "Egon", LastName = "Mueller", Contact = string.Format("em-{0}@acme.org", i) };
						pagedWriter.StoreAsync(user).ConfigureAwait(false).GetAwaiter().GetResult();
					}
				}

				var endDate = DateTime.Now;
				
				Console.WriteLine("Took {0} seconds", (endDate- startDate).TotalSeconds);
			}
        }
        
        static void CheckMaxItems(string storageKey, string storageSecret) 
        {
			using (var storageContext = new StorageContext(storageKey, storageSecret))
			{
				storageContext.AddAttributeMapper(typeof(UserModel2), "DemoUserModel2");
				storageContext.CreateTable<UserModel2>(true);

				var items = storageContext.Query<UserModel2>(5).AsEnumerable();
				Console.WriteLine("Found {0} items", items.Count());
				if (items.Count() != 5)
					throw new Exception("Wrong Item Count");
			}
        }

		static void TestReadInterfaceValues(string storageKey, string storageSecret)
		{
			// create a new user
			var user01 = new UserModel3() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
			var user02 = new UserModel3() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
			user02.Codes.Add(new Code() { CodeType = "x1", CodeValue = "x2" });
			user02.Codes.Add(new Code() { CodeType = "x3", CodeValue = "x4" });


			using (var storageContext = new StorageContext(storageKey, storageSecret))
			{
				// set the delegate 
				storageContext.SetDelegate(new DemoDelegate());
				
				// ensure we are using the attributes
				storageContext.AddAttributeMapper(typeof(UserModel3));

				// insert the model
				storageContext.EnableAutoCreateTable().MergeOrInsert<UserModel3>(user01);
				storageContext.EnableAutoCreateTable().MergeOrInsert<UserModel3>(user02);

				// query all
				var result = storageContext.Query<UserModel3>();

				foreach (var r in result)
				{
					Console.WriteLine("{0}: {1}", r.LastName, r.Codes.Count());
				}
			}
		}
    }

	public class DemoDelegate : IStorageContextDelegate
	{
		public void OnQueryed(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery, Exception e)
		{	
			
		}

		public void OnQuerying(Type modelType, string partitionKey, string rowKey, int maxItems, bool isContinuationQuery)
		{	
			Console.WriteLine("Query for {0}: P: {1}, R: {2}, C: {3}", modelType.ToString(), partitionKey, rowKey, isContinuationQuery ? "Yes": "No");				
		}

		public void OnStored(Type modelType, nStoreOperation storaeOperationType, int modelCount, Exception e)
		{		
		}

		public void OnStoring(Type modelType, nStoreOperation storaeOperationType)
		{		
			Console.WriteLine("Store for {0}: O: {1}", modelType.ToString(), storaeOperationType.ToString());				
		}
	}
}
