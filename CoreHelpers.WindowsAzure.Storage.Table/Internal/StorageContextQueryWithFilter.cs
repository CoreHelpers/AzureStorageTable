using System;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Serialization;

namespace CoreHelpers.WindowsAzure.Storage.Table.Internal
{
    public class StorageContextQueryWithFilter<T> : StorageContextQueryNow<T>, IStorageContextQueryWithFilter<T> where T : class, new()
    {
        public StorageContextQueryWithFilter(StorageContext context, StorageContextQueryNow<T> src, string rowKey)
            : base(context, src)
        {
            _optionalRowKey = rowKey;
        }

        public IStorageContextQueryWithFilter<T> Filter(IEnumerable<QueryFilter> filters)
        {
            if (filters == null)
                return Filter(String.Empty);

            var builder = new TableQueryFilterBuilder(filters);                          
            return Filter(builder.Build());
        }

        public IStorageContextQueryWithFilter<T> Filter(string filter)
        {
            var withFilter = new StorageContextQueryWithFilter<T>(_context, this, _optionalRowKey);
            withFilter._optionalFilter = filter;
            withFilter._optionalMaxItems = _optionalMaxItems;
            return withFilter;
        }

        public IStorageContextQueryWithFilter<T> LimitTo(int maxItems)
        {
            var withFilter = new StorageContextQueryWithFilter<T>(_context, this, _optionalRowKey);
            withFilter._optionalFilter = _optionalFilter;
            withFilter._optionalMaxItems = maxItems;
            return withFilter;
        }                
    }
}

