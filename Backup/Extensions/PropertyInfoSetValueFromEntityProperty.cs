using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;

namespace CoreHelpers.WindowsAzure.Storage.Table.Extensions
{
	public static class PropertyInfoSetValueFromEntityProperty
	{
				
		public static void SetValueFromEntityProperty(this PropertyInfo property, Object entity, EntityProperty entityProperty) 
		{
			if (entityProperty.IsNull())
			{
				property.SetOrAddValue(entity, null, false);
			}
			else
			{
				// define the propertytype 							
				var isCollection = (property.PropertyType.GetTypeInfo().GetInterface("IList") != null);					
				var propertyType = property.PropertyType;

				// handle colleciton s
				if (isCollection)
					propertyType = property.PropertyType.GenericTypeArguments[0];
									
				switch (entityProperty.PropertyType)
				{
					case EdmType.String:
						if (propertyType != typeof(string))
							break;						

						property.SetOrAddValue(entity, entityProperty.StringValue, isCollection);
						break;
					case EdmType.Binary:
						if (propertyType != typeof(byte[]))						
							break;						

						property.SetOrAddValue(entity, entityProperty.BinaryValue, isCollection);
						break;
					case EdmType.Boolean:
						if (propertyType != typeof(bool) && propertyType != typeof(bool?))						
							break;						

						property.SetOrAddValue(entity, entityProperty.BooleanValue, isCollection);
						break;
					case EdmType.DateTime:
						if (propertyType == typeof(DateTime))
						{
							property.SetOrAddValue(entity, entityProperty.DateTimeOffsetValue.Value.UtcDateTime, isCollection);
						}
						else if (propertyType == typeof(DateTime?))
						{
							property.SetOrAddValue(entity, entityProperty.DateTimeOffsetValue.HasValue ? entityProperty.DateTimeOffsetValue.Value.UtcDateTime : (DateTime?)null, isCollection);
						}
						else if (propertyType == typeof(DateTimeOffset))
						{
							property.SetOrAddValue(entity, entityProperty.DateTimeOffsetValue.Value, isCollection);
						}
						else if (propertyType == typeof(DateTimeOffset?))
						{
							property.SetOrAddValue(entity, entityProperty.DateTimeOffsetValue, isCollection);
						}

						break;
					case EdmType.Double:
						if (propertyType != typeof(double) && propertyType != typeof(double?))						
							break;						

						property.SetOrAddValue(entity, entityProperty.DoubleValue, isCollection);
						break;
					case EdmType.Guid:
						if (propertyType != typeof(Guid) && propertyType != typeof(Guid?))						
							break;						

						property.SetOrAddValue(entity, entityProperty.GuidValue, isCollection);
						break;
					case EdmType.Int32:
						if (propertyType != typeof(int) && propertyType != typeof(int?) &&
							propertyType != typeof(double) && propertyType != typeof(double?))												
							break;

						if (propertyType == typeof(double) || propertyType == typeof(double?))
							property.SetOrAddValue(entity, Convert.ToDouble(entityProperty.Int32Value), isCollection);
						else												
							property.SetOrAddValue(entity, entityProperty.Int32Value, isCollection);
							
						break;
					case EdmType.Int64:
						if (propertyType != typeof(long) && propertyType != typeof(long?))						
							break;						

						property.SetOrAddValue(entity, entityProperty.Int64Value, isCollection);
						break;
				}
			}		
		}
	}
}
