using System;
using System.Collections;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class StoreAsJsonObjectAttribute : StoreAsAttribute
	{	
		protected Type ObjectType { get; set; }
		
		public StoreAsJsonObjectAttribute() 
		{}
			
		public StoreAsJsonObjectAttribute(Type objectType) 
		{
			ObjectType = objectType;
		}
		
		public override EntityProperty ConvertToEntityProperty(PropertyInfo property, object obj)
		{
			// get the value 
			var element = property.GetValue(obj);
            if (element == null)
                return null;

			// convert to strong 
			var stringifiedElement = JsonConvert.SerializeObject(element);

			// create entity property
			return new EntityProperty(stringifiedElement);
		}
		
		public override Object ConvertFromEntityProperty(PropertyInfo property, EntityProperty entityProperty)
		{			
			if (ObjectType != null && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(ObjectType) && ObjectType.GetTypeInfo().UnderlyingSystemType != null)
			{				
				var convertedElements = JsonConvert.DeserializeObject(entityProperty.StringValue, ObjectType);
				try
				{
					return Activator.CreateInstance(property.PropertyType, convertedElements);
				} catch(MissingMethodException)
                {
					return Activator.CreateInstance(ObjectType, convertedElements);
				}
			} else if (ObjectType != null)
			{
				return JsonConvert.DeserializeObject(entityProperty.StringValue, ObjectType);
			}
			else
			{
				return JsonConvert.DeserializeObject(entityProperty.StringValue, property.PropertyType);
			}
		}
	}
}
