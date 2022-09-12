using System;
using CoreHelpers.WindowsAzure.Storage.Table;

namespace CoreHelpers.WindowsAzure.Storage.Table.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class VirtualRowKeyAttribute : Attribute
	{
		public string RowKeyFormat { get; set; }

        public nVirtualValueEncoding Encoding { get; set; }

        public VirtualRowKeyAttribute(string RowKeyFormat, nVirtualValueEncoding Encoding = nVirtualValueEncoding.None) {
			this.RowKeyFormat = RowKeyFormat;
			this.Encoding = Encoding;
		}
	}
}
