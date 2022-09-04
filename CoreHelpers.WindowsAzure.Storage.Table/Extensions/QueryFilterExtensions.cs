using System;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;

namespace CoreHelpers.WindowsAzure.Storage.Table.Extensions
{
    public static class QueryFilterExtensions
    {        
        public static string ToFilterString(this QueryFilter filter)
        {
            var filterOperation = "eq";
            switch (filter.Operator)
            {
                case QueryFilterOperator.Equal:
                    filterOperation = "eq";
                    break;
                case QueryFilterOperator.NotEqual:
                    filterOperation = "ne";
                    break;
                case QueryFilterOperator.Lower:
                    filterOperation = "lt";
                    break;
                case QueryFilterOperator.Greater:
                    filterOperation = "gt";
                    break;
                case QueryFilterOperator.LowerEqual:
                    filterOperation = "le";
                    break;
                case QueryFilterOperator.GreaterEqual:
                    filterOperation = "ge";
                    break;
            }

            var filterValueString = default(string);

            if (filter.Value is string)
                filterValueString = $"'{(string)filter.Value}'";
            else if (filter.Value is bool)
                filterValueString = ((bool)filter.Value) ? "true" : "false";
            else if (filter.Value is byte[])
                filterValueString = Convert.ToString((byte[]) filter.Value);            
            else if (filter.Value is DateTimeOffset)
                filterValueString = Convert.ToString((DateTimeOffset) filter.Value);
            else if (filter.Value is double)
                filterValueString = Convert.ToString((double) filter.Value);
            else if (filter.Value is Guid)
                filterValueString = ((Guid) filter.Value).ToString();
            else if (filter.Value is int)
                filterValueString = Convert.ToString((int) filter.Value);
            else if (filter.Value is long)
                filterValueString = Convert.ToString((long) filter.Value);
            else
                throw new NotSupportedException($"QueryFilter of Type \"{filter.Value?.GetType().FullName}\" is not supported.");

            return $"{filter.Property} {filterOperation} {filterValueString}";
        }
    }
}