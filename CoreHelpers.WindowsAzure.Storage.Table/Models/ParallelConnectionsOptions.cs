using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHelpers.WindowsAzure.Storage.Table.Models
{
    public class ParallelConnectionsOptions
    {

        public static ParallelConnectionsOptions Default => new ParallelConnectionsOptions()
        {
            RunInParallel = true,
            MaxDegreeOfParallelism = 20
        };

        public bool RunInParallel { get; set; }
        
        public int MaxDegreeOfParallelism { get; set; }

    }
}
