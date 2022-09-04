using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HandlebarsDotNet;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class VirtualListAttribute : Attribute, IVirtualTypeAttribute
    {
		private HandlebarsTemplate<object, string> TemplateFunction { get; set; }
		private string DigitFormat { get; set; }
		
		public VirtualListAttribute(string PropertyFormat, int Digits)
		{
			// generate the template
			this.TemplateFunction = Handlebars.Compile(PropertyFormat);
			
			this.DigitFormat = "";
			for (int i = 0; i < Digits; i++)
				this.DigitFormat += "0";
		}

        public void WriteProperty<T>(PropertyInfo propertyInfo, T obj, TableEntityBuilder builder)
        {
            // get the value
            var arrayValue = propertyInfo.GetValue(obj);

            // check if enumerable 
            if ((arrayValue as IList) == null)
                return;

            // visit every element
            for (int idx = 0; idx < (arrayValue as IList).Count; idx++)
            {
                // get the element 
                var element = (arrayValue as IList)[idx];

                // generate the property name
                var propertyName = TemplateFunction(new { index = idx.ToString(DigitFormat) });
				
				// write the property
				builder.AddProperty(propertyName, element);                
            }
        }

        public void ReadProperty<T>(Azure.Data.Tables.TableEntity dataObject, PropertyInfo propertyInfo, T obj)
        {
			// set the current index 
			var currentIndex = 0;

            // try to read every single index
			while(true) 
            {
                // get the propertyname
                var propertyName = TemplateFunction(new { index = currentIndex.ToString(DigitFormat) });

                // check if we have the name 
                if (!dataObject.ContainsKey(propertyName))
                    break;

				// read the value
				var dataObjectPropertyValue = dataObject[propertyName];

				// read the value 				
				propertyInfo.SetOrAddValue(obj, dataObjectPropertyValue, true);

				// increase the index
				currentIndex++;

            }         
        }

        
    }
}
