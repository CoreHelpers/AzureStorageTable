using System;

namespace CoreHelpers.WindowsAzure.Storage.Table.Abstractions
{
    public class QueryFilter
    {
        public string Property { get; set; }
        
        public object Value { get; set; }

        public QueryFilterOperator Operator { get; set; }

        //TODO find smoother way to connect multiple filters
        public QueryFilterType FilterType { get; set; }
    }
}