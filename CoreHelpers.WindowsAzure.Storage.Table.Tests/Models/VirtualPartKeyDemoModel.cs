using System;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{
	[Storable()]
	[VirtualPartitionKey("{{Value1}}-{{Value2}}")]
	[VirtualRowKey("{{Value2}}-{{Value3}}")]
	public class VirtualPartKeyDemoModel
	{
		public string Value1 { get; set; } = String.Empty;
		public string Value2 { get; set; } = String.Empty;
        public string Value3 { get; set; } = String.Empty;
    }
}
