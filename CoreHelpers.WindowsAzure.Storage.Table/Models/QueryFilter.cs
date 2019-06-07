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
        public string Value { get; set;  }
        public QueryFilterOperator Operator { get; set; }
        public QueryFilterType FilterType { get; set;  }

        public virtual string FilterString {
            get {
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
                return TableQuery.GenerateFilterCondition(Property, filterOperation, Value); 
            }
        } 
    }
}
