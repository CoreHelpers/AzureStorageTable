using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.DemoCases;
using CoreHelpers.WindowsAzure.Storage.Table.Demo.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
			
			// read the config
			var configLocation = Path.Combine("..", "Credentials.json");
			JObject config = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(configLocation));

			// register all demo cases
			var cases = new List<IDemoCase>
			{
				// new UC01StoreWithStaticEntityMapper(),
                // new UC02StoreWithAttributeMapper(),
                // new UC03StoreWithAttributeMapperManualRegistration(),
                // new UC04GetVirtualArray(),
                // new UC05StoreAsJson(),
                // new UC06AutoCreateTable(),
                // new UC07CreateModelsPaged(),
                // new UC08CheckMaxItems(),
                // new UC09ReadInterfaceValues(),
                // new UC10CreateHugeAmountOfDemoEntries(),
                // new UC11ReadPageByPage(),
                // new UC12PartialUpdateMergeModel(),
                // new UC13DynamicallyCreateList(),
                // new UC14WriteNullList(),
                // new UC15DynamicTableNameChange(),
                // new UC16Backup()
                new UC17Restore()
            };
			
			// register demo cases for Ger Cloud
			var casesGer = new List<IDemoCase>
			{
				new UC06AutoCreateTable()				
			};
			
			// execute in WW cloud 
			Console.WriteLine("Executing Demo Cases (WW Cloud)");
			foreach (var useCase in cases)
				await useCase.Execute(config.GetValue("key").ToString(), config.GetValue("secret").ToString());									
				
			// execute in GER cloud 
			/*Console.WriteLine("Executing Demo Cases (GER Cloud)");
			foreach (var useCase in casesGer)
				await useCase.Execute(config.GetValue("keyde").ToString(), config.GetValue("secretde").ToString(), "core.cloudapi.de");													*/
        }                            				
    }	
}
