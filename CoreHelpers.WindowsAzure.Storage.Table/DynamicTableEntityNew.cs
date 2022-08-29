using System;
using System.Reflection;
using System.Collections.Generic;
using HandlebarsDotNet;
using Azure.Data.Tables;
using Azure;
using ITableEntity = Azure.Data.Tables.ITableEntity;
using TableEntity = Azure.Data.Tables.TableEntity;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using System.Runtime.Serialization;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;
using System.Collections;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    internal class DynamicTableEntityNew<T> : ITableEntity, IDictionary<string, object> where T : new()
    {
        private T _srcModel { get; set; }
        private DynamicTableEntityMapper _entityMapper { get; set; }

        public DynamicTableEntityNew()
        {
            _srcModel = new T();
            BuildPropertyInfoMap();
        }

        public DynamicTableEntityNew(T src, DynamicTableEntityMapper entityMapper)
        {
            _srcModel = src;
            _entityMapper = entityMapper;            
            ETag = new ETag("*");
            BuildPropertyInfoMap();
        }

        public ETag ETag { get; set; }


        public string PartitionKey
        {
            get { return GetTableStorageDefaultProperty<string>(_entityMapper.PartitionKeyFormat); }
            set { SetTableStorageDefaultProperty<string, PartitionKeyAttribute>(value); }
        }

        public string RowKey
        {
            get { return GetTableStorageDefaultProperty<string>(_entityMapper.RowKeyFormat); }
            set { SetTableStorageDefaultProperty<string, RowKeyAttribute>(value); }
        }

        public DateTimeOffset? Timestamp { get; set; }

        public ICollection<string> Keys
        {
            get
            {
                return _propertyInfoKeys.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        private S GetTableStorageDefaultProperty<S>(string format) where S : class
        {
            if (typeof(S) == typeof(string) && format.Contains("{{") && format.Contains("}}"))
            {
                var template = Handlebars.Compile(format);
                return template(_srcModel) as S;
            }
            else
            {
                var propertyInfo = _srcModel.GetType().GetRuntimeProperty(format);
                return propertyInfo.GetValue(_srcModel) as S;
            }
        }

        private void SetTableStorageDefaultProperty<S, A>(S value) where A : Attribute
        {
            foreach (var property in _srcModel.GetType()?.GetRuntimeProperties())
            {
                if (property.GetCustomAttribute<A>() != null && property?.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                {
                    var setter = property.GetSetMethod();
                    if (setter != null && !setter.IsStatic)
                        property.SetValue(_srcModel, value);
                }
            }            
        }

        internal static bool ShouldSkipProperty(PropertyInfo property)
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

            // properties with [IgnoreDataMember]
            if (property.GetCustomAttribute(typeof(IgnoreDataMemberAttribute)) != null)
            {
                // Logger.LogInformational(operationContext, SR.TraceIgnoreAttribute, property.Name);
                return true;
            }

            return false;
        }

        private Dictionary<string, PropertyInfo> _propertyInfoKeys = new Dictionary<string, PropertyInfo>();

        private void BuildPropertyInfoMap()
        {
            _propertyInfoKeys = new Dictionary<string, PropertyInfo>();

            IEnumerable<PropertyInfo> objectProperties = _srcModel.GetType().GetTypeInfo().GetProperties();

            foreach (PropertyInfo property in objectProperties)
            {
                if (ShouldSkipProperty(property))
                {
                    continue;
                }

                _propertyInfoKeys.Add(property.Name, property);
            }
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return _propertyInfoKeys.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            // never happens
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            // never happens
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new DynamicTableEntityNewEnumerator<T>(_srcModel, _propertyInfoKeys);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    internal class DynamicTableEntityNewEnumerator<T> : IEnumerator<KeyValuePair<string, object>> where T: new()
    {
        private Dictionary<string, PropertyInfo> _propertyMap;
        private IEnumerator<KeyValuePair<string, PropertyInfo>> _propertyMapEnumerator;

        private T _model;

        
        public DynamicTableEntityNewEnumerator(T model, Dictionary<string, PropertyInfo> propertyMap)
        {
            _propertyMap = propertyMap;
            _model = model;
            _propertyMapEnumerator = _propertyMap.GetEnumerator();
        }

        public KeyValuePair<string, object> Current {
            get {                
                return new KeyValuePair<string, object>(
                    _propertyMapEnumerator.Current.Key,
                    _propertyMapEnumerator.Current.Value.GetValue(_model)
                    );
            }            
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public void Dispose()
        {
            _propertyMapEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            return _propertyMapEnumerator.MoveNext();
        }

        public void Reset()
        {
            _propertyMapEnumerator.Reset();
        }
    }
}
