using System;
using System.Collections;
using System.Reflection;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using Newtonsoft.Json;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class StoreEnumAsStringAttribute : Attribute, IVirtualTypeAttribute
    {	
		protected Type ObjectType { get; set; }
		
		public StoreEnumAsStringAttribute() 
		{}
			
		public StoreEnumAsStringAttribute(Type objectType) 
		{
			ObjectType = objectType;
		}
			
        public void WriteProperty<T>(PropertyInfo propertyInfo, T obj, TableEntityBuilder builder)
        {
            // get the value 
            var element = propertyInfo.GetValue(obj);
            if (element == null)
                return;

            // convert to strong 
            var stringifiedElement = (element as Enum)?.ToString("F");

			// add the property
			builder.AddProperty(propertyInfo.Name, stringifiedElement);
        }

        public void ReadProperty<T>(Azure.Data.Tables.TableEntity dataObject, PropertyInfo propertyInfo, T obj)
        {
			// check if we have the property in our entity othetwise move forward
			if (!dataObject.ContainsKey(propertyInfo.Name))
				return;

            // get the string value
            var stringValue = Convert.ToString(dataObject[propertyInfo.Name]);

            // Handle null/empty stored values quickly
            // Nullable type would be defaulted to null already
            // non-nullable cannot be set to null
            if (string.IsNullOrEmpty(stringValue))
                return;

            // Support nullable enum properties by parsing against the underlying enum type
            var propType = propertyInfo.PropertyType;
            var enumType = Nullable.GetUnderlyingType(propType) ?? propType;

            // Parse the enum stringValue name (stored as string) using the resolved enum type
            var parsed = Enum.Parse(enumType, stringValue);
            propertyInfo.SetValue(obj, parsed);
        }
    }
}
