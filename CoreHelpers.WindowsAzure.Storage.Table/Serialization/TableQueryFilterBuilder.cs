using System;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Extensions;

namespace CoreHelpers.WindowsAzure.Storage.Table.Serialization
{
    internal class TableQueryFilterBuilder
    {
        private List<QueryFilter> filters = new List<QueryFilter>();
        private string postFix;


        public TableQueryFilterBuilder () { }

        public TableQueryFilterBuilder(IEnumerable<QueryFilter> filters)
        {
            this.filters = new List<QueryFilter>(filters);
        }

        public TableQueryFilterBuilder Attach(string filter)
        {
            postFix = filter;
            return this;
        }

        public TableQueryFilterBuilder Where(string property, QueryFilterOperator operation, string value)
        {
            filters.Add(new QueryFilter() { FilterType = QueryFilterType.Where, Operator = operation, Property = property, Value = value });
            return this;
        }

        public TableQueryFilterBuilder And(string property, QueryFilterOperator operation, string value)
        {
            filters.Add(new QueryFilter() { FilterType = QueryFilterType.And, Operator = operation, Property = property, Value = value });
            return this;
        }

        public TableQueryFilterBuilder Or(string property, QueryFilterOperator operation, string value)
        {
            filters.Add(new QueryFilter() { FilterType = QueryFilterType.Or, Operator = operation, Property = property, Value = value });
            return this;
        }

        public string Build()
        {
            var resultString = String.Empty;

            foreach (var filter in filters)
            {
                if (String.IsNullOrEmpty(resultString))
                    resultString = $"({filter.ToFilterString()})";
                else
                {
                    var combineOperation = filter.FilterType == QueryFilterType.Or ? "or" : "and";
                    resultString += $" {combineOperation} ({filter.ToFilterString()})";
                }
            }

            if (!String.IsNullOrEmpty(postFix))
            {
                if (string.IsNullOrEmpty(resultString))
                    resultString = postFix;
                else
                    resultString = $"{resultString} and {postFix}";
            }

            return resultString;
        }
    }
}

