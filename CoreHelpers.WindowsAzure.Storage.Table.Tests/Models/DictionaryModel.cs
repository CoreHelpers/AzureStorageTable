using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
    [Storable(Tablename = "DictionaryModel")]
    public class DictionaryModel
    {
        [PartitionKey]        
        [RowKey]
        public string Id { get; set; }

        [StoreAsJsonObject(typeof(Dictionary<string, string>))]
        public IDictionary<string, string> Propertiers { get; set; } = new Dictionary<string, string>();
    }
}

