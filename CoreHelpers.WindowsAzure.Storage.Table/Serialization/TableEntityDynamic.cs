using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Azure.Data.Tables;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using HandlebarsDotNet;

namespace CoreHelpers.WindowsAzure.Storage.Table.Serialization
{
    public static class TableEntityDynamic
    {        
        public static TableEntity ToEntity<T>(T model, StorageEntityMapper entityMapper) where T: new()
        {
            var builder = new TableEntityBuilder();

            // set the keys
            builder.AddPartitionKey(GetTableStorageDefaultProperty<string, T>(entityMapper.PartitionKeyFormat, model));
            builder.AddRowKey(GetTableStorageDefaultProperty<string, T>(entityMapper.RowKeyFormat, model));

            // get all properties from model 
            IEnumerable<PropertyInfo> objectProperties = model.GetType().GetTypeInfo().GetProperties();

            // visit all properties
            foreach (PropertyInfo property in objectProperties)
            {
                if (ShouldSkipProperty(property))
                    continue;

                // check if we have a special convert attached via attribute if so generate the required target 
                // properties with the correct converter
                var virtualTypeAttribute = property.GetCustomAttributes().Where(a => a is IVirtualTypeAttribute).Select(a => a as IVirtualTypeAttribute).FirstOrDefault<IVirtualTypeAttribute>();
                if (virtualTypeAttribute != null)
                    virtualTypeAttribute.WriteProperty<T>(property, model, builder);                                                   
                else
                    builder.AddProperty(property.Name, property.GetValue(model, null));                                                       
            }
                
            // build the result 
            return builder.Build();
        }

        public static T fromEntity<T>(TableEntity entity, StorageEntityMapper entityMapper) where T : class, new()
        {
            // create the target model
            var model = new T();

            // get all properties from model 
            IEnumerable<PropertyInfo> objectProperties = model.GetType().GetTypeInfo().GetProperties();
            
            // visit all properties
            foreach (PropertyInfo property in objectProperties)
            {
                if (ShouldSkipProperty(property))
                    continue;
               
                // check if we have a special convert attached via attribute if so generate the required target 
                // properties with the correct converter
                var virtualTypeAttribute = property.GetCustomAttributes().Where(a => a is IVirtualTypeAttribute).Select(a => a as IVirtualTypeAttribute).FirstOrDefault<IVirtualTypeAttribute>();
                if (virtualTypeAttribute != null)
                    virtualTypeAttribute.ReadProperty<T>(entity, property, model);
                else
                {
                    if (!entity.ContainsKey(property.Name))
                        continue;

                    var objectValue = default(object);

                    if (!entity.TryGetValue(property.Name, out objectValue))
                        continue;

                    if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTimeOffset) || property.PropertyType == typeof(DateTimeOffset?) )
                        property.SetDateTimeOffsetValue(model, objectValue);
                    else
                        property.SetValue(model, objectValue);
                }
            }

            return model;
        }

        private static S GetTableStorageDefaultProperty<S, T>(string format, T model) where S : class
        {
            if (typeof(S) == typeof(string) && format.Contains("{{") && format.Contains("}}"))
            {
                var template = Handlebars.Compile(format);
                return template(model) as S;
            }
            else
            {
                var propertyInfo = model.GetType().GetRuntimeProperty(format);
                return propertyInfo.GetValue(model) as S;
            }
        }


        private static bool ShouldSkipProperty(PropertyInfo property)
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
            if (property.GetCustomAttribute(typeof(IgnoreDataMemberAttribute)) != null)
            {
                // Logger.LogInformational(operationContext, SR.TraceIgnoreAttribute, property.Name);
                return true;
            }

            return false;
        }
    }
}

