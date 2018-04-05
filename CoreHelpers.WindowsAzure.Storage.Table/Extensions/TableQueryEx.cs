using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHelpers.WindowsAzure.Storage.Table.Extensions
{
    public class TableQueryEx
    {
        public static string CheckAndCombineFilters(string filterA, string operatorString, string filterB)
        {
            if (string.IsNullOrWhiteSpace(filterA))
                return filterB;

            return TableQuery.CombineFilters(filterA, operatorString, filterB);
        }

        public static string CombineFilters(IEnumerable<string> filters, string operatorString)
        {
            return string.Join(
                            " " + operatorString + " ",
                            filters
                        );
        }
    }
}
