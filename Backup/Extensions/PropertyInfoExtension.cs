using System;
using System.Collections;
using System.Reflection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Extensions
{
	public static class PropertyInfoExtension
	{
		public static void SetOrAddValue(this PropertyInfo propertyInfo, object obj, object val, bool isCollection) 
		{
			if (isCollection) {
				// get the collection 
				var collection = propertyInfo.GetValue(obj) as IList;

				// add the value 
				collection.Add(val);
			} else
				propertyInfo.SetValue(obj, val, null);
		}
	}
}
