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

			// prepare the value
			var resultValue = Enum.Parse(propertyInfo.PropertyType, stringValue);


			// set the value
			propertyInfo.SetValue(obj, resultValue);
        }
    }
}
