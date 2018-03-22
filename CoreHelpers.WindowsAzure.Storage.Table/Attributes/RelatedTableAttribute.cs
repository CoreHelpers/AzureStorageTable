using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
    public class RelatedTableAttribute : Attribute
    {
        public Type Type {
            get;
            set;
        }

        public RelatedTableAttribute()
        {
        }

        public RelatedTableAttribute(Type type)
        {
            this.Type = type;
        }
    }
}
