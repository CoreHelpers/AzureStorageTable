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
    }
}
