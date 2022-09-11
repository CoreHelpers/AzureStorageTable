using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Azure.Data.Tables;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using HandlebarsDotNet;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
    public class VirtualDictionaryAttribute : Attribute, IVirtualTypeAttribute
    {
        private string _propertyPrefix;

        public VirtualDictionaryAttribute(string PropertyPrefix)
        {
            _propertyPrefix = PropertyPrefix;
        }

        public void ReadProperty<T>(TableEntity dataObject, PropertyInfo propertyInfo, T obj)
        {
            // search all properties with the prefix
            var keys = dataObject.Keys.Where(k => k.StartsWith(_propertyPrefix));
            
            // visit every key
            foreach(var key in keys)
            {
                // check if we have the name 
                if (!dataObject.ContainsKey(key))
                    continue;

                // prepare the key value
                var keyStringValue = key.Remove(0, _propertyPrefix.Length);

                // read the value
                var dataObjectPropertyValue = dataObject[key];

                // get the dictionary 
                var dictionary = propertyInfo.GetValue(obj) as IDictionary;

                // work with the type
                var arguments = dictionary.GetType().GetGenericArguments();
                var keyType = arguments[0];
                var valueType = arguments[1];

                var keyValue = default(object);

                switch(keyType.ToString())
                {
                    case "System.Int32":
                        keyValue = Convert.ToInt32(keyStringValue);
                        break;
                    default:
                        keyValue = keyStringValue;
                        break;
                }

                dictionary.Add(keyValue, dataObjectPropertyValue);
            }            
        }

        public void WriteProperty<T>(PropertyInfo propertyInfo, T obj, TableEntityBuilder builder)
        {
            // get the value
            var dictionaryValue = propertyInfo.GetValue(obj);

            // check if enumerable 
            if ((dictionaryValue as IDictionary) == null)
                return;

            // visit every element
            foreach(DictionaryEntry kvp in (dictionaryValue as IDictionary))
            {
                // generate the property name
                var propertyName = $"{_propertyPrefix}{kvp.Key}";

                // write the property
                builder.AddProperty(propertyName, kvp.Value);                
            }           
        }
    }
}

