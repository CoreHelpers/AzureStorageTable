using System;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
	public interface IDemo2
	{
		string Value { get; set; }
	}

	public class JDemo2 : IDemo2
	{
		public string Value { get; set; } = String.Empty;
    }

	[Storable(Tablename: "JObjectModel")]
	public class JObjectModel
	{
		[PartitionKey]
		[RowKey]
		public string UUID { get; set; } = String.Empty;

        [StoreAsJsonObject]
		public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

		[StoreAsJsonObject(typeof(JDemo2))]
		public IDemo2 Data2 { get; set; } = new JDemo2() { Value = "333" };
	}

	[Storable(Tablename: "JObjectModel")]
	public class JObjectModelVerify
	{
		[PartitionKey]
		[RowKey]
		public string UUID { get; set; } = String.Empty;

        public String Data { get; set; } = String.Empty;

        public String Data2 { get; set; } = String.Empty;
    }
}
