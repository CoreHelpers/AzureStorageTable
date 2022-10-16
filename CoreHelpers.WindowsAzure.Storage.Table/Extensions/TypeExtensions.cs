using System;
using System.Linq;

namespace CoreHelpers.WindowsAzure.Storage.Table.Extensions
{
    public enum ExportEdmType
    {
        String,
        Binary,
        Boolean,
        DateTime,
        Double,
        Guid,
        Int32,
        Int64
    }

    public static class TypeExtensions
    {
        public static ExportEdmType GetEdmPropertyType(this Type type)
        {
            if (type == typeof(string))
                return ExportEdmType.String;
            else if (type == typeof(byte[]))
                return ExportEdmType.Binary;
            else if (type == typeof(Boolean) || type == typeof(bool))
                return ExportEdmType.Boolean;
            else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
                return ExportEdmType.DateTime;
            else if (type == typeof(Double))
                return ExportEdmType.Double;
            else if (type == typeof(Guid))
                return ExportEdmType.Guid;
            else if (type == typeof(Int32) || type == typeof(int))
                return ExportEdmType.Int32;
            else if (type == typeof(Int64))
                return ExportEdmType.Int64;
            else
                throw new NotImplementedException($"Datatype {type.ToString()} not supporter");
        }

        public static bool IsDerivedFromGenericParent(this Type type, Type parentType)
        {
            if (!parentType.IsGenericType)
            {
                throw new ArgumentException("type must be generic", "parentType");
            }
            if (type == null || type == typeof(object))
            {
                return false;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == parentType)
            {
                return true;
            }
            return type.BaseType.IsDerivedFromGenericParent(parentType)
                || type.GetInterfaces().Any(t => t.IsDerivedFromGenericParent(parentType));
        }
    }
}




