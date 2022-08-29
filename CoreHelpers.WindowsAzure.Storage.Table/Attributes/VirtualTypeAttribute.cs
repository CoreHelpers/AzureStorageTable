using System;
using System.Collections.Generic;
using System.Reflection;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using Microsoft.WindowsAzure.Storage.Table;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	public abstract class VirtualTypeAttribute : Attribute
	{
		public abstract void WriteProperty(PropertyInfo propertyInfo, Object obj, Dictionary<string, EntityProperty> targetList);

		public abstract void ReadProperty(PropertyInfo propertyInfo, Object obj, IDictionary<string, EntityProperty> entityProperties);		
	}
}
