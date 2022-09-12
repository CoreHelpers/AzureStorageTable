using System;
using System.Collections.Generic;
using Azure.Data.Tables;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;

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

        public TableEntityBuilder AddRowKey(string rkey, nVirtualValueEncoding encoding = nVirtualValueEncoding.None)
        {

            switch (encoding)
            {
                case nVirtualValueEncoding.None:
                    _data.Add("RowKey", rkey);
                    break;
                case nVirtualValueEncoding.Base64:
                    _data.Add("RowKey", rkey.ToBase64());
                    break;
                case nVirtualValueEncoding.Sha256:
                    _data.Add("RowKey", rkey.ToSha256());
                    break;
            }
            
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

