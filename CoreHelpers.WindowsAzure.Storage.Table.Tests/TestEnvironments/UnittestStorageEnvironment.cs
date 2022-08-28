using System;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.TestEnvironments
{
    public class UnittestStorageEnvironment : ITestEnvironment
    {       
        public string ConnectionString {
            get {

                var connectionString = Environment.GetEnvironmentVariable("STORAGE");
                if (!String.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("Using environment credentials");
                    return connectionString;
                }

                var filePath = Environment.ExpandEnvironmentVariables(Path.Combine("%HOME%", ".corehelpers.credentials.txt"));
                Console.WriteLine("Using filesystem credentials");
                return File.ReadLines(filePath).First();
            }
        }
    }
}

