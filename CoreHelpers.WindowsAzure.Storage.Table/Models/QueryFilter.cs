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

            if (Value is string)
            {
                return TableQuery.GenerateFilterCondition(Property, filterOperation, (string) Value);
            }

            if (Value is bool)
            {
                return TableQuery.GenerateFilterConditionForBool(Property, filterOperation, (bool) Value);
            }

            if (Value is byte[])
            {
                return TableQuery.GenerateFilterConditionForBinary(Property, filterOperation, (byte[]) Value);
            }

            if (Value is DateTimeOffset)
            {
                return TableQuery.GenerateFilterConditionForDate(Property, filterOperation, (DateTimeOffset) Value);
            }

            if (Value is double)
            {
                return TableQuery.GenerateFilterConditionForDouble(Property, filterOperation, (double) Value);
            }

            if (Value is Guid)
            {
                return TableQuery.GenerateFilterConditionForGuid(Property, filterOperation, (Guid) Value);
            }

            if (Value is int)
            {
                return TableQuery.GenerateFilterConditionForInt(Property, filterOperation, (int) Value);
            }

            if (Value is long)
            {
                return TableQuery.GenerateFilterConditionForLong(Property, filterOperation, (long) Value);
            }

            throw new NotSupportedException($"QueryFilter of Type \"{Value?.GetType().FullName}\" is not supported.");
        }
    }
}