using System;
namespace CoreHelpers.WindowsAzure.Storage.Table
{	
	public class VirtualRowKeyAttribute : Attribute
	{
		public string RowKeyFormat { get; set; }
		
		public VirtualRowKeyAttribute(string RowKeyFormat) {
			this.RowKeyFormat = RowKeyFormat;
		}
	}
}
