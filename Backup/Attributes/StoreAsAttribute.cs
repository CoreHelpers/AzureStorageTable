using System;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	public abstract class StoreAsAttribute : Attribute
	{
		public abstract EntityProperty ConvertToEntityProperty(PropertyInfo property, Object obj); 
				
		public abstract Object ConvertFromEntityProperty(PropertyInfo property, EntityProperty entityProperty); 								
	}
}
