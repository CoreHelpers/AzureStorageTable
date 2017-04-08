using System;
namespace CoreHelpers.WindowsAzure.Storage.Table
{
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
