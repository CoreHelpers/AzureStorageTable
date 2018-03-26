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

                // initialize the collection dynamicall if required 
                if (collection == null) 
                {
                    collection = Activator.CreateInstance(propertyInfo.PropertyType) as IList;
                    propertyInfo.SetValue(obj, collection);
                }                    
                
				// add the value 
				collection.Add(val);
			} else
				propertyInfo.SetValue(obj, val, null);
		}
        
        public static bool IsGenericOfType(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.GetTypeInfo().BaseType;
            }
            return false;
        }
    }
}
