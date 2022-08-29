using System;
using System.Collections.Generic;
using Azure.Data.Tables;

namespace CoreHelpers.WindowsAzure.Storage.Table.Serialization
{
    public class TableEntityBuilder
    {
        private IDictionary<string, object> _data = new Dictionary<string, object>();


        public TableEntityBuilder AddPartitionKey(string pkey)
        {
            _data.Add("PartitionKey", pkey);
            return this;
        }

        public TableEntityBuilder AddRowKey(string rkey)
        {
            _data.Add("RowKey", rkey);
            return this;
        }

        public TableEntityBuilder AddProperty(string property, object value)
        {
            _data.Add(property, value);
            return this;
        }

        public TableEntity Build()
        {
            return new TableEntity(_data);
        }
    }
}

