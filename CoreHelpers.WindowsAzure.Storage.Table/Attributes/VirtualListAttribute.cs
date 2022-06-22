using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HandlebarsDotNet;
using Microsoft.WindowsAzure.Storage.Table;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class VirtualListAttribute : VirtualTypeAttribute
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

		public override void WriteProperty(PropertyInfo propertyInfo, Object obj, Dictionary<string, EntityProperty> targetList)
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
				
				// generate the property object
				EntityProperty newProperty = EntityProperty.CreateEntityPropertyFromObject(element);
				if (newProperty != null)
					targetList.Add(propertyName, newProperty);								
			}										
		}

		public override void ReadProperty(PropertyInfo propertyInfo, object obj, IDictionary<string, EntityProperty> entityProperties)
		{
			// what is the max amount of poperties
			var maxIndex = entityProperties.Count;

			// try to read every single index
			for (int idx = 0; idx < maxIndex; idx++) 
			{
				// get the propertyname
				var propertyName = TemplateFunction(new { index = idx.ToString(DigitFormat) });

				// check if we have the name 
				if (!entityProperties.ContainsKey(propertyName))
					break;

				// get the entityproperty
				var entityProperty = entityProperties[propertyName];

				// read the value 				
				propertyInfo.SetValueFromEntityProperty(obj, entityProperty);	
			
			}			
		}
	}
}
