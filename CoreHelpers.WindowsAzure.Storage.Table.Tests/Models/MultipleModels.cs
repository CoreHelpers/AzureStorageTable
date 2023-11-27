using System;
using System.Collections.Generic;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Models
{

    [Storable(TypeField = nameof(Type))]
    public class MultipleModelsBase
    {
        [PartitionKey]
        public string P { get; set; } = "Partition01";

        [RowKey]
        public string Contact { get; set; } = String.Empty;

    }

	public class MultipleModels1 : MultipleModelsBase
	{
        public string Model1Field { get; set; } = String.Empty;

	}

    public class MultipleModels2 : MultipleModelsBase
    {
        public string Model2Field { get; set; } = String.Empty;

    }

}
