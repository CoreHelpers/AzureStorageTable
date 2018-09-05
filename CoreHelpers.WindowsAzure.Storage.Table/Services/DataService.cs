using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Services
{
    internal abstract class DataService
    {
        public const string PartitionKey = nameof(PartitionKey);
        public const string RowKey = nameof(RowKey);

        protected readonly StorageContext storageContext;

        public DataService(StorageContext storageContext)
        {
            this.storageContext = storageContext;
        }

        protected EntityProperty GenerateProperty(EdmType propertyValueType, object propertyValueStr)
        {
            EntityProperty propertyValue;
            switch (propertyValueType)
            {
                case EdmType.String:
                    propertyValue = EntityProperty.GeneratePropertyForString((string)propertyValueStr);
                    break;
                case EdmType.Binary:
                    propertyValue = EntityProperty.GeneratePropertyForByteArray(Convert.FromBase64String(propertyValueStr.ToString()));
                    break;
                case EdmType.Boolean:
                    propertyValue = EntityProperty.GeneratePropertyForBool(Convert.ToBoolean(propertyValueStr));
                    break;
                case EdmType.DateTime:
                    propertyValue = EntityProperty.GeneratePropertyForDateTimeOffset((DateTime)propertyValueStr);
                    break;
                case EdmType.Double:
                    propertyValue = EntityProperty.GeneratePropertyForDouble(Convert.ToDouble(propertyValueStr));
                    break;
                case EdmType.Guid:
                    propertyValue = EntityProperty.GeneratePropertyForGuid(Guid.Parse(propertyValueStr.ToString()));
                    break;
                case EdmType.Int32:
                    propertyValue = EntityProperty.GeneratePropertyForInt(Convert.ToInt32(propertyValueStr));
                    break;
                case EdmType.Int64:
                    propertyValue = EntityProperty.GeneratePropertyForLong(Convert.ToInt64(propertyValueStr));
                    break;
                default: throw new ArgumentException($"Can't create table property with Type {string.Join(", ", propertyValueType)} and value {propertyValueStr}");
            }
            return propertyValue;
        }

        protected object GetPropertyValue(EdmType propertyValueType, EntityProperty value)
        {
            object propertyValue;
            switch (propertyValueType)
            {
                case EdmType.String:
                    propertyValue = value.StringValue;
                    break;
                case EdmType.Binary:
                    propertyValue = value.BinaryValue;
                    break;
                case EdmType.Boolean:
                    propertyValue = value.BooleanValue;
                    break;
                case EdmType.DateTime:
                    propertyValue = value.DateTime;
                    break;
                case EdmType.Double:
                    propertyValue = value.DoubleValue;
                    break;
                case EdmType.Guid:
                    propertyValue = value.GuidValue;
                    break;
                case EdmType.Int32:
                    propertyValue = value.Int32Value;
                    break;
                case EdmType.Int64:
                    propertyValue = value.Int64Value;
                    break;
                default: throw new ArgumentException($"Can't create table property with Type {string.Join(", ", propertyValueType)} and value {value.ToString()}");
            }
            return propertyValue;
        }
    }
}
