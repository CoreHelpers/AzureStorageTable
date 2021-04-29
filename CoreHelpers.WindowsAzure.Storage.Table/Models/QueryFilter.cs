using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace CoreHelpers.WindowsAzure.Storage.Table.Models
{
    public enum QueryFilterOperator
    {
        Equal,
        NotEqual,
        Greater,
        Lower,
        GreaterEqual,
        LowerEqual
    }

    public enum QueryFilterType
    {
        Where,
        And,
        Or
    }

    public class QueryFilter
    {
        public string Property { get; set; }
        
        public object Value { get; set; }

        public QueryFilterOperator Operator { get; set; }

        //TODO find smoother way to connect multiple filters
        public QueryFilterType FilterType { get; set; }

        public override string ToString()
        {
            var filterOperation = QueryComparisons.Equal;
            switch (Operator)
            {
                case QueryFilterOperator.Equal:
                    filterOperation = QueryComparisons.Equal;
                    break;
                case QueryFilterOperator.NotEqual:
                    filterOperation = QueryComparisons.NotEqual;
                    break;
                case QueryFilterOperator.Lower:
                    filterOperation = QueryComparisons.LessThan;
                    break;
                case QueryFilterOperator.Greater:
                    filterOperation = QueryComparisons.GreaterThan;
                    break;
                case QueryFilterOperator.LowerEqual:
                    filterOperation = QueryComparisons.LessThanOrEqual;
                    break;
                case QueryFilterOperator.GreaterEqual:
                    filterOperation = QueryComparisons.GreaterThanOrEqual;
                    break;
            }

            if (Value is string stringValue)
            {
                return TableQuery.GenerateFilterCondition(Property, filterOperation, stringValue);
            }

            if (Value is bool boolValue)
            {
                return TableQuery.GenerateFilterConditionForBool(Property, filterOperation, boolValue);
            }

            if (Value is byte[] binaryValue)
            {
                return TableQuery.GenerateFilterConditionForBinary(Property, filterOperation, binaryValue);
            }

            if (Value is DateTimeOffset dateValue)
            {
                return TableQuery.GenerateFilterConditionForDate(Property, filterOperation, dateValue);
            }

            if (Value is double doubleValue)
            {
                return TableQuery.GenerateFilterConditionForDouble(Property, filterOperation, doubleValue);
            }

            if (Value is Guid guidValue)
            {
                return TableQuery.GenerateFilterConditionForGuid(Property, filterOperation, guidValue);
            }

            if (Value is int intValue)
            {
                return TableQuery.GenerateFilterConditionForInt(Property, filterOperation, intValue);
            }

            if (Value is long longValue)
            {
                return TableQuery.GenerateFilterConditionForLong(Property, filterOperation, longValue);
            }

            throw new NotSupportedException($"QueryFilter of Type \"{Value?.GetType().FullName}\" is not supported.");
        }
    }
}