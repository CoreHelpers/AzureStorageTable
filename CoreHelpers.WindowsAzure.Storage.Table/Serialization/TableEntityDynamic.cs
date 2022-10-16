using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Azure.Data.Tables;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Internal;
using HandlebarsDotNet;
using Newtonsoft.Json.Linq;

namespace CoreHelpers.WindowsAzure.Storage.Table.Serialization
{
    internal static class TableEntityDynamic
    {
        public static TableEntity ToEntity<T>(T model, IStorageContext context) where T : new()
        {
            if (context as StorageContext == null)
                throw new Exception("Invalid interface implementation");
            else
                return TableEntityDynamic.ToEntity<T>(model, (context as StorageContext).GetEntityMapper<T>(), context);
        }

        public static TableEntity ToEntity<T>(T model, StorageEntityMapper entityMapper, IStorageContext context) where T : new()
        {
            var builder = new TableEntityBuilder();

            // set the keys
            builder.AddPartitionKey(GetTableStorageDefaultProperty<string, T>(entityMapper.PartitionKeyFormat, model));
            builder.AddRowKey(GetTableStorageDefaultProperty<string, T>(entityMapper.RowKeyFormat, model), entityMapper.RowKeyEncoding);

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

                var relatedTableAttribute = property.GetCustomAttributes().Where(a => a is RelatedTableAttribute).Select(a => a as RelatedTableAttribute).FirstOrDefault<RelatedTableAttribute>();

                if (virtualTypeAttribute != null)
                    virtualTypeAttribute.WriteProperty<T>(property, model, builder);
                else if (relatedTableAttribute != null)
                    continue;
                else
                    builder.AddProperty(property.Name, property.GetValue(model, null));
            }

            // build the result 
            return builder.Build();
        }

        public static T fromEntity<T>(TableEntity entity, StorageEntityMapper entityMapper, IStorageContext context) where T : class, new()
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

                var relatedTableAttribute = property.GetCustomAttributes().Where(a => a is RelatedTableAttribute).Select(a => a as RelatedTableAttribute).FirstOrDefault<RelatedTableAttribute>();

                if (virtualTypeAttribute != null)
                    virtualTypeAttribute.ReadProperty<T>(entity, property, model);
                else if (relatedTableAttribute != null)
                    property.SetValue(model, LoadRelatedTableProperty(context, model, objectProperties, property));
                else
                {
                    if (!entity.ContainsKey(property.Name))
                        continue;

                    var objectValue = default(object);

                    if (!entity.TryGetValue(property.Name, out objectValue))
                        continue;

                    if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTimeOffset) || property.PropertyType == typeof(DateTimeOffset?))
                        property.SetDateTimeOffsetValue(model, objectValue);
                    else
                        property.SetValue(model, objectValue);
                }
            }

            return model;
        }

        private static object LoadRelatedTableProperty<T>(IStorageContext context, T model, IEnumerable<PropertyInfo> objectProperties, PropertyInfo property) where T : class, new()
        {
            var relatedTable = property.GetCustomAttribute<RelatedTableAttribute>();
            var isLazy = false;
            var isEnumerable = false;


            Type endType;
            if (property.PropertyType.IsDerivedFromGenericParent(typeof(Lazy<>)))
            {
                endType = property.PropertyType.GetTypeInfo().GenericTypeArguments[0];
                isLazy = true;
            }
            else
                endType = property.PropertyType;

            if (endType.IsDerivedFromGenericParent(typeof(IEnumerable<>)))
                isEnumerable = true;

            // determine the partition key
            string extPartition = relatedTable.PartitionKey;
            if (!string.IsNullOrWhiteSpace(extPartition))
            {
                // if the partition key is the name of a property on the model, get the value
                var partitionProperty = objectProperties.Where((pi) => pi.Name == relatedTable.PartitionKey).FirstOrDefault();
                if (partitionProperty != null)
                    extPartition = partitionProperty.GetValue(model).ToString();
            }

            string extRowKey = relatedTable.RowKey ?? endType.Name;
            // if the row key is the name of a property on the model, get the value
            var rowkeyProperty = objectProperties.Where((pi) => pi.Name == extRowKey).FirstOrDefault();
            if (rowkeyProperty != null)
                extRowKey = rowkeyProperty.GetValue(model).ToString();

            var method = typeof(StorageContext).GetMethod(nameof(StorageContext.QueryAsync),
                isEnumerable ?
                    new[] { typeof(string), typeof(int) } :
                    new[] { typeof(string), typeof(string), typeof(int) });
            var generic = method.MakeGenericMethod(endType);

            // if the property is a lazy type, create the lazy initialization
            if (isLazy)
            {
                var lazyType = typeof(DynamicLazy<>);
                var constructed = lazyType.MakeGenericType(endType);

                object o = Activator.CreateInstance(constructed, new Func<object>(() =>
                {
                    var waitable = (dynamic)generic.Invoke(context, new object[] { extPartition, extRowKey, 1 });
                    return waitable.Result;
                }));
                return o;

            }
            else
            {
                var waitable = (dynamic)generic.Invoke(context, new object[] { extPartition, extRowKey, 1 });
                return waitable.Result;
            }
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

