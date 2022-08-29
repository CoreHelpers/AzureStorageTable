using System;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using System.Reflection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
    public interface IVirtualTypeAttribute
    {
        void WriteProperty<T>(PropertyInfo propertyInfo, T obj, TableEntityBuilder builder);
    }
}

