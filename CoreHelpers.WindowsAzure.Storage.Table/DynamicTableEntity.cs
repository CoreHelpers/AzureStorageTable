using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using HandlebarsDotNet;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;

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

            // properties with [RelatedTable]
            if (property.GetCustomAttribute(typeof(RelatedTableAttribute)) != null)
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

				// check if we have a special convert attached via attribute if so generate the required target 
				// properties with the correct converter
				if (property.GetCustomAttribute<VirtualTypeAttribute>() != null)
				{
					var typeConvert = property.GetCustomAttribute<VirtualTypeAttribute>();
					typeConvert.WriteProperty(property, entity, retVals);					
				}
				else if (property.GetCustomAttribute<StoreAsAttribute>() != null)
				{
					var typeConvert = property.GetCustomAttribute<StoreAsAttribute>();
					var newProperty = typeConvert.ConvertToEntityProperty(property, entity);

                    if (newProperty != null)
					    retVals.Add(property.Name, newProperty);
				}
				else
				{
					EntityProperty newProperty = EntityProperty.CreateEntityPropertyFromObject(property.GetValue(entity, null));

					// property will be null if unknown type
					if (newProperty != null)					
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

				// check if we have a special convert attached via attribute if so generate the required target 
				// properties with the correct converter
				if (property.GetCustomAttribute<VirtualTypeAttribute>() != null)
				{
					var typeConvert = property.GetCustomAttribute<VirtualTypeAttribute>();
					typeConvert.ReadProperty(property, entity, properties);
				}				
				else
				{
					// only proceed with properties that have a corresponding entry in the dictionary
					if (!properties.ContainsKey(property.Name))
					{
						// Logger.LogInformational(operationContext, SR.TraceMissingDictionaryEntry, property.Name);
						continue;
					}

					EntityProperty entityProperty = properties[property.Name];
					
					if (property.GetCustomAttribute<StoreAsAttribute>() != null)
					{
						var typeConvert = property.GetCustomAttribute<StoreAsAttribute>();
						var model = typeConvert.ConvertFromEntityProperty(property, entityProperty);						
						property.SetValue(entity, model);																
					}
					else
					{						
						property.SetValueFromEntityProperty(entity, entityProperty);
					}
				}
			}
		}
	}
}
