using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHelpers.WindowsAzure.Storage.Table.Internal
{
    internal class DynamicLazy<T> : Lazy<T>
    {
        public DynamicLazy(Func<object> factory) : base(() => (T)factory())
        {

        }

    }
}