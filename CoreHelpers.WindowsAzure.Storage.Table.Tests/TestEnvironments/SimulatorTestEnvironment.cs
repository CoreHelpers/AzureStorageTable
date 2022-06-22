using System;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.TestEnvironments
{
    public class SimulatorTestEnvironment : ITestEnvironment
    {
        public string ConnectionString { get; } = "UseDevelopmentStorage=true";

        
    }
}

