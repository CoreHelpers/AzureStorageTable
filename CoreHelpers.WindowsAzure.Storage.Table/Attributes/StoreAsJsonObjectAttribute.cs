using System;
using System.Collections;
using System.Reflection;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using Newtonsoft.Json;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class StoreAsJsonObjectAttribute : Attribute, IVirtualTypeAttribute
    {	
		protected Type ObjectType { get; set; }
		
		public StoreAsJsonObjectAttribute() 
		{}
			
		public StoreAsJsonObjectAttribute(Type objectType) 
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
            var stringifiedElement = JsonConvert.SerializeObject(element);

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
			var resultValue = default(Object);

			// handle the special operations
            if (ObjectType != null && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(ObjectType) && ObjectType.GetTypeInfo().UnderlyingSystemType != null)
            {
                var convertedElements = JsonConvert.DeserializeObject(stringValue, ObjectType);
                try
                {
                    resultValue = Activator.CreateInstance(propertyInfo.PropertyType, convertedElements);
                }
                catch (MissingMethodException)
                {
                    resultValue = Activator.CreateInstance(ObjectType, convertedElements);
                }
            }
            else if (ObjectType != null)
            {
                resultValue = JsonConvert.DeserializeObject(stringValue, ObjectType);
            }
            else
            {
                resultValue = JsonConvert.DeserializeObject(stringValue, propertyInfo.PropertyType);
            }

			// set the value
			propertyInfo.SetValue(obj, resultValue);
        }
    }
}
