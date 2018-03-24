using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoreHelpers.WindowsAzure.Storage.Table
{
    internal class RelatedTableItem<T> where T : new()
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; }

        public DynamicTableEntity<T> Model { get; set; }

        public PropertyInfo Property { get; set; }
    }
}
