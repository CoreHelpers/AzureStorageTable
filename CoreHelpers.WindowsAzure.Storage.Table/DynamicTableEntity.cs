using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using HandlebarsDotNet;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
	internal class DynamicTableEntity<T> : ITableEntity where T : new()
	{
		private T _srcModel { get; set; }
		private DynamicTableEntityMapper _entityMapper { get; set; }

		public DynamicTableEntity()
		{
			_srcModel = new T();
		}

		public DynamicTableEntity(T src, DynamicTableEntityMapper entityMapper)
		{
			_srcModel = src;
			_entityMapper = entityMapper;
			ETag = "*";
		}

		public T Model { 
			get { return _srcModel;  }
		}

		public string ETag { get; set; }

		public string PartitionKey
		{
			get
			{
				if ( _entityMapper.PartitionKeyFormat.Contains("{{") && _entityMapper.PartitionKeyFormat.Contains("}}")) 
				{
					var template = Handlebars.Compile(_entityMapper.PartitionKeyFormat);
					return template(_srcModel);
				} else {
					var propertyInfo = _srcModel.GetType().GetRuntimeProperty(_entityMapper.PartitionKeyFormat);
					return propertyInfo.GetValue(_srcModel) as String;
				}					
			}

			set
			{
				// we don't need to do anything 
			}
		}

		public string RowKey
		{
			get
			{
				if ( _entityMapper.RowKeyFormat.Contains("{{") && _entityMapper.RowKeyFormat.Contains("}}")) 
				{
					var template = Handlebars.Compile(_entityMapper.RowKeyFormat);
					return template(_srcModel);
				} else {
					var propertyInfo = _srcModel.GetType().GetRuntimeProperty(_entityMapper.RowKeyFormat);
					return propertyInfo.GetValue(_srcModel) as String;
				}				
			}

			set
			{
				// we don't need to do anything 
			}
		}

		public DateTimeOffset Timestamp { get; set; }

		public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
		{
			ReflectionRead(_srcModel, properties, operationContext);
		}

		public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
		{
			return ReflectionWrite(_srcModel, operationContext);
		}

		/// <summary>
		/// Determines if the given property should be skipped based on its name, if it exposes a public getter and setter, and if the IgnoreAttribute is not defined.
		/// </summary>
		/// <param name="property">The PropertyInfo of the property to check</param>
		/// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
		/// <returns>True if the property should be skipped, false otherwise. </returns>
		internal static bool ShouldSkipProperty(PropertyInfo property, OperationContext operationContext)
		{
			// reserved properties
			string propName = property.Name;
			if (propName == TableConstants.PartitionKey ||
				propName == TableConstants.RowKey ||
				propName == TableConstants.Timestamp ||
				propName == TableConstants.Etag)
			{
				return true;
			}


			MethodInfo setter = property.SetMethod;
			MethodInfo getter = property.GetMethod;

			// Enforce public getter / setter
			if (setter == null || !setter.IsPublic || getter == null || !getter.IsPublic)
			{
				// Logger.LogInformational(operationContext, SR.TraceNonPublicGetSet, property.Name);
				return true;
			}

			// Skip static properties
			if (setter.IsStatic)
			{
				return true;
			}

			// properties with [IgnoreAttribute]
		       	if (property.GetCustomAttribute(typeof(IgnorePropertyAttribute)) != null)
			{
				// Logger.LogInformational(operationContext, SR.TraceIgnoreAttribute, property.Name);
				return true;
			}

			return false;
		}

		private static IDictionary<string, EntityProperty> ReflectionWrite(object entity, OperationContext operationContext)
		{
			Dictionary<string, EntityProperty> retVals = new Dictionary<string, EntityProperty>();

			IEnumerable<PropertyInfo> objectProperties = entity.GetType().GetTypeInfo().GetProperties();            		

			foreach (PropertyInfo property in objectProperties)
			{
				if (ShouldSkipProperty(property, operationContext))
				{
					continue;
				}

				EntityProperty newProperty = EntityProperty.CreateEntityPropertyFromObject(property.GetValue(entity, null));

				// property will be null if unknown type
				if (newProperty != null)
				{
					retVals.Add(property.Name, newProperty);
				}
			}

			return retVals;
		}

		private static void ReflectionRead(object entity, IDictionary<string, EntityProperty> properties, OperationContext operationContext)
		{
			IEnumerable<PropertyInfo> objectProperties = entity.GetType().GetTypeInfo().GetProperties();

			foreach (PropertyInfo property in objectProperties)
			{
				if (ShouldSkipProperty(property, operationContext))
				{
					continue;
				}

				// only proceed with properties that have a corresponding entry in the dictionary
				if (!properties.ContainsKey(property.Name))
				{
					// Logger.LogInformational(operationContext, SR.TraceMissingDictionaryEntry, property.Name);
					continue;
				}

				EntityProperty entityProperty = properties[property.Name];

				//if (entityProperty.IsNull)
				//{
				//	property.SetValue(entity, null, null);
				//}
				//else
				{
					switch (entityProperty.PropertyType)
					{
						case EdmType.String:
							if (property.PropertyType != typeof(string))
							{
								continue;
							}

							property.SetValue(entity, entityProperty.StringValue, null);
							break;
						case EdmType.Binary:
							if (property.PropertyType != typeof(byte[]))
							{
								continue;
							}

							property.SetValue(entity, entityProperty.BinaryValue, null);
							break;
						case EdmType.Boolean:
							if (property.PropertyType != typeof(bool) && property.PropertyType != typeof(bool?))
							{
								continue;
							}

							property.SetValue(entity, entityProperty.BooleanValue, null);
							break;
						case EdmType.DateTime:
							if (property.PropertyType == typeof(DateTime))
							{
								property.SetValue(entity, entityProperty.DateTimeOffsetValue.Value.UtcDateTime, null);
							}
							else if (property.PropertyType == typeof(DateTime?))
							{
								property.SetValue(entity, entityProperty.DateTimeOffsetValue.HasValue ? entityProperty.DateTimeOffsetValue.Value.UtcDateTime : (DateTime?)null, null);
							}
							else if (property.PropertyType == typeof(DateTimeOffset))
							{
								property.SetValue(entity, entityProperty.DateTimeOffsetValue.Value, null);
							}
							else if (property.PropertyType == typeof(DateTimeOffset?))
							{
								property.SetValue(entity, entityProperty.DateTimeOffsetValue, null);
							}

							break;
						case EdmType.Double:
							if (property.PropertyType != typeof(double) && property.PropertyType != typeof(double?))
							{
								continue;
							}

							property.SetValue(entity, entityProperty.DoubleValue, null);
							break;
						case EdmType.Guid:
							if (property.PropertyType != typeof(Guid) && property.PropertyType != typeof(Guid?))
							{
								continue;
							}

							property.SetValue(entity, entityProperty.GuidValue, null);
							break;
						case EdmType.Int32:
							if (property.PropertyType != typeof(int) && property.PropertyType != typeof(int?))
							{
								continue;
							}

							property.SetValue(entity, entityProperty.Int32Value, null);
							break;
						case EdmType.Int64:
							if (property.PropertyType != typeof(long) && property.PropertyType != typeof(long?))
							{
								continue;
							}

							property.SetValue(entity, entityProperty.Int64Value, null);
							break;
					}
				}
			}
		}
	}
}
