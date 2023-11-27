using System;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
    public class StorableAttribute : Attribute
    {
        public string Tablename { get; set; }

        public string TypeField { get; set; } = null;
    
        public StorableAttribute() {}
        
        public StorableAttribute(string Tablename, string TypeField)
        {
            this.Tablename = Tablename;
            this.TypeField = TypeField;
        }

        public StorableAttribute(string Tablename)
        {
            this.Tablename = Tablename;
        }
    }
}
