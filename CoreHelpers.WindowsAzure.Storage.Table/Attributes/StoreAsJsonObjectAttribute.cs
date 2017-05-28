using System;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class StoreAsJsonObjectAttribute : StoreAsAttribute
	{		
		public override EntityProperty ConvertToEntityProperty(PropertyInfo property, object obj)
		{
			// get the value 
			var element = property.GetValue(obj);

			// convert to strong 
			var stringifiedElement = JsonConvert.SerializeObject(element);

			// create entity property
			return new EntityProperty(stringifiedElement);
		}
		
		public override Object ConvertFromEntityProperty(PropertyInfo property, EntityProperty entityProperty)
		{				
			return JsonConvert.DeserializeObject(entityProperty.StringValue, property.PropertyType);			
		}
	}
}
