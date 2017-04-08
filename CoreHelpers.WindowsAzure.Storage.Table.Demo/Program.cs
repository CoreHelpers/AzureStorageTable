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

            // StorageWithStaticEntityMapper(config.GetValue("key").ToString(),config.GetValue("secret").ToString());
            StorageWithAttributeMapper(config.GetValue("key").ToString(),config.GetValue("secret").ToString());
        }

	static void StorageWithStaticEntityMapper(string storageKey, string storageSecret) 
	{
		// create a new user
		var user = new UserModel() { FirstName = "Egon", LastName = "Mueller", Contact = "em@acme.org" };
		user.Contact = user.Contact + Guid.NewGuid().ToString();
		
		using (var storageContext = new StorageContext(storageKey, storageSecret))
		{
			// configure the entity mapper
			storageContext.AddEntityMapper(typeof(UserModel), new DynamicTableEntityMapper() { TableName = "UserProfiles", PartitionKeyPropery = "Contact", RowKeyProperty = "Contact" });
		
			// ensure the table exists
			storageContext.CreateTable<UserModel>();
		
			// inser the model
			storageContext.MergeOrInsert<UserModel>(user);
		
			// query all
			var result = storageContext.Query<UserModel>();
		
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
    }
}
