using System;
namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts
{
    public interface ITestEnvironment
    {
        string ConnectionString
        {
            get;
        }
    }
}

