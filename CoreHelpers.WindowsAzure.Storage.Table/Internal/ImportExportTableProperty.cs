using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHelpers.WindowsAzure.Storage.Table.Internal
{
    internal class ImportExportTablePropertyEntity
    {
        public ImportExportTablePropertyEntity(string name, int type, object value)
        {
            PropertyName = name;
            PropertyType = type;
            PropertyValue = value;
        }
        public string PropertyName { get; set; }
        public int PropertyType { get; set; }
        public object PropertyValue { get; set; }
    }
}
