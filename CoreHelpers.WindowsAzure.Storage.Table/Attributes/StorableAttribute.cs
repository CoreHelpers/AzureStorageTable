using System;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
    public class StorableAttribute : Attribute
    {
        public string Tablename { get; set; }
    
        public StorableAttribute() {}
        
        public StorableAttribute(string Tablename)
        {
            this.Tablename = Tablename;
        }
    }
}
