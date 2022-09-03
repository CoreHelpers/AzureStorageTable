using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace CoreHelpers.WindowsAzure.Storage.Table.Extensions
{
    public static class QueryFilterExtensions
    {
        public static string ToFilterString(this QueryFilter filter)
        {
            var filterOperation = QueryComparisons.Equal;
            switch (filter.Operator)
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

            if (filter.Value is string)
            {
                return TableQuery.GenerateFilterCondition(filter.Property, filterOperation, (string) filter.Value);
            }

            if (filter.Value is bool)
            {
                return TableQuery.GenerateFilterConditionForBool(filter.Property, filterOperation, (bool) filter.Value);
            }

            if (filter.Value is byte[])
            {
                return TableQuery.GenerateFilterConditionForBinary(filter.Property, filterOperation, (byte[]) filter.Value);
            }

            if (filter.Value is DateTimeOffset)
            {
                return TableQuery.GenerateFilterConditionForDate(filter.Property, filterOperation, (DateTimeOffset) filter.Value);
            }

            if (filter.Value is double)
            {
                return TableQuery.GenerateFilterConditionForDouble(filter.Property, filterOperation, (double) filter.Value);
            }

            if (filter.Value is Guid)
            {
                return TableQuery.GenerateFilterConditionForGuid(filter.Property, filterOperation, (Guid) filter.Value);
            }

            if (filter.Value is int)
            {
                return TableQuery.GenerateFilterConditionForInt(filter.Property, filterOperation, (int) filter.Value);
            }

            if (filter.Value is long)
            {
                return TableQuery.GenerateFilterConditionForLong(filter.Property, filterOperation, (long) filter.Value);
            }

            throw new NotSupportedException($"QueryFilter of Type \"{filter.Value?.GetType().FullName}\" is not supported.");
        }
    }
}