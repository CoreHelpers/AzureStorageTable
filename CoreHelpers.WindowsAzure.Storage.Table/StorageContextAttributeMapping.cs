using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using System.Collections.Generic;
using System.Reflection;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public partial class StorageContext : IStorageContext
    {
        private Dictionary<Type, StorageEntityMapper> _entityMapperRegistry { get; set; } = new Dictionary<Type, StorageEntityMapper>();

        public void AddEntityMapper(Type entityType, StorageEntityMapper entityMapper)
            => _entityMapperRegistry.Add(entityType, entityMapper);

        public void AddEntityMapper(Type entityType, string partitionKeyFormat, string rowKeyFormat, string tableName)
        {
            _entityMapperRegistry.Add(entityType, new StorageEntityMapper()
            {
                PartitionKeyFormat = partitionKeyFormat,
                RowKeyFormat = rowKeyFormat,
                TableName = tableName
            });
        }

        public void RemoveEntityMapper(Type entityType)
        {
            if (_entityMapperRegistry.ContainsKey(entityType))
                _entityMapperRegistry.Remove(entityType);
        }

        public void AddAttributeMapper()
        {
            AddAttributeMapper(Assembly.GetEntryAssembly());
            AddAttributeMapper(Assembly.GetCallingAssembly());
        }

        internal void AddAttributeMapper(Assembly assembly)
        {
            var typesWithAttribute = assembly.GetTypesWithAttribute(typeof(StorableAttribute));
            foreach (var type in typesWithAttribute)
            {
                AddAttributeMapper(type);
            }
        }

        public void AddAttributeMapper<T>(String optionalTablenameOverride = null) where T : class
        {
            AddAttributeMapper(typeof(T), optionalTablenameOverride);
        }

        public void AddAttributeMapper(Type type)
        {
            AddAttributeMapper(type, string.Empty);
        }

        public void AddAttributeMapper(Type type, String optionalTablenameOverride)
        {
            // get the concrete attribute
            var storableAttribute = type.GetTypeInfo().GetCustomAttribute<StorableAttribute>();
            if (String.IsNullOrEmpty(storableAttribute.Tablename))
            {
                storableAttribute.Tablename = type.Name;
            }

            // store the neded properties
            string partitionKeyFormat = null;
            string rowKeyFormat = null;

            // get the partitionkey property & rowkey property
            var properties = type.GetRuntimeProperties();
            foreach (var property in properties)
            {
                if (partitionKeyFormat != null && rowKeyFormat != null)
                    break;

                if (partitionKeyFormat == null && property.GetCustomAttribute<PartitionKeyAttribute>() != null)
                    partitionKeyFormat = property.Name;

                if (rowKeyFormat == null && property.GetCustomAttribute<RowKeyAttribute>() != null)
                    rowKeyFormat = property.Name;
            }

            // virutal partition key property
            var virtualPartitionKeyAttribute = type.GetTypeInfo().GetCustomAttribute<VirtualPartitionKeyAttribute>();
            if (virtualPartitionKeyAttribute != null && !String.IsNullOrEmpty(virtualPartitionKeyAttribute.PartitionKeyFormat))
                partitionKeyFormat = virtualPartitionKeyAttribute.PartitionKeyFormat;

            // virutal row key property
            var virtualRowKeyAttribute = type.GetTypeInfo().GetCustomAttribute<VirtualRowKeyAttribute>();
            if (virtualRowKeyAttribute != null && !String.IsNullOrEmpty(virtualRowKeyAttribute.RowKeyFormat))
                rowKeyFormat = virtualRowKeyAttribute.RowKeyFormat;

            // check 
            if (partitionKeyFormat == null || rowKeyFormat == null)
                throw new Exception("Missing Partition or RowKey Attribute");

            // build the mapper
            AddEntityMapper(type, new StorageEntityMapper()
            {
                TableName = String.IsNullOrEmpty(optionalTablenameOverride) ? storableAttribute.Tablename : optionalTablenameOverride,
                PartitionKeyFormat = partitionKeyFormat,
                RowKeyFormat = rowKeyFormat
            });
        }

        public IEnumerable<Type> GetRegisteredMappers()
        {
            return _entityMapperRegistry.Keys;
        }

        public StorageEntityMapper GetEntityMapper<T>()
        {
            return _entityMapperRegistry[typeof(T)];
        }
    }
}