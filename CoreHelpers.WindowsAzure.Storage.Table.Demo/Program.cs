using System;
using System.IO;
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
    }
}
