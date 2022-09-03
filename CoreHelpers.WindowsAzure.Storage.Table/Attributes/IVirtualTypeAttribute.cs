using System;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;
using System.Reflection;
using Azure.Data.Tables;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
    public interface IVirtualTypeAttribute
    {
        void WriteProperty<T>(PropertyInfo propertyInfo, T obj, TableEntityBuilder builder);

        void ReadProperty<T>(TableEntity dataObject, PropertyInfo propertyInfo, T obj);
    }
}

