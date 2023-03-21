using System;
using System.Globalization;

namespace CoreHelpers.WindowsAzure.Storage.Table.Extensions
{
    public static class QueryFilterExtensions
    {
        public static string ToFilterString(this QueryFilter filter)
        {
            var filterOperation = filter.Operator switch
            {
                QueryFilterOperator.Equal => "eq",
                QueryFilterOperator.NotEqual => "ne",
                QueryFilterOperator.Lower => "lt",
                QueryFilterOperator.Greater => "gt",
                QueryFilterOperator.LowerEqual => "le",
                QueryFilterOperator.GreaterEqual => "ge",
                _ => "eq"
            };

            var filterValueString = filter.Value switch
            {
                string value => $"'{value}'",
                bool b => b.ToString().ToLower(),
                byte[] bytes => $"binary'{Convert.ToBase64String(bytes)}'",
                DateTimeOffset offset => $"datetime'{offset.ToUniversalTime():s}Z'",
                double d => d.ToString(CultureInfo.InvariantCulture),
                Guid guid => $"guid'{guid}'",
                int i => i.ToString(),
                long l => $"{l}L",
                _ => throw new NotSupportedException(
                    $"QueryFilter of Type \"{filter.Value?.GetType().FullName}\" is not supported.")
            };

            return $"{filter.Property} {filterOperation} {filterValueString}";
        }
    }
}