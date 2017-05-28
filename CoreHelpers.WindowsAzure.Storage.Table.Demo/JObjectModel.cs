using System;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo
{
	[Storable(Tablename: "JObjectModel")]
	public class JObjectModel
	{
		[PartitionKey]
		[RowKey]
		public string UUID { get; set; }

		[StoreAsJsonObject]
		public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
	}
}
