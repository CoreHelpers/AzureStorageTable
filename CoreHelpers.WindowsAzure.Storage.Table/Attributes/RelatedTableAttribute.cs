using System;
namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
    public class RelatedTableAttribute : Attribute
    {
        /// <summary>
        /// The partitionkey of the related table, if this is the name of a property on the model the property value will be used.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// The rowkey of the related table, if this is a property on the model, the property value will be loaded, if it is empty this will default to the name of the type.
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// On saving the parent entity also save the related tables, currently not supported with null values in <see cref="RowKey"/>, Defaults to False.
        /// </summary>
        public bool AutoSave { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitionKey">The partitionkey of the related table, if this is the name of a property on the model the property value will be used.</param>
        public RelatedTableAttribute(string partitionKey)
        {
            PartitionKey = partitionKey;
            AutoSave = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitionKey">The partitionkey of the related table, if this is the name of a property on the model the property value will be used.</param>
        /// <param name="rowKey">The rowkey of the related table, if this is a property on the model, the property value will be loaded, if it is empty this will default to the name of the type.</param>
        public RelatedTableAttribute(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            AutoSave = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitionKey">The partitionkey of the related table, if this is the name of a property on the model the property value will be used.</param>
        /// <param name="rowKey">The rowkey of the related table, if this is a property on the model, the property value will be loaded, if it is empty this will default to the name of the type.</param>
        /// <param name="autoSave">Sets the autosave property</param>
        public RelatedTableAttribute(string partitionKey, string rowKey, bool autoSave)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            AutoSave = autoSave;
        }

    }
}