using System;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo
{
	[Storable(Tablename: "VArrayModels")]
	public class VArrayModel
	{
		[PartitionKey]
		[RowKey]
		public string UUID { get; set; }

		[VirtualList(PropertyFormat: "DE{{index}}", Digits: 2)]
		public List<int> DataElements { get; set; } = new List<int>();
	}
}
