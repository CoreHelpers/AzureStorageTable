using System;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests.TestEnvironments
{
    public class CredentialsFilesEnvironment : ITestEnvironment
    {       
        public string ConnectionString {
            get {

                var filePath = Environment.ExpandEnvironmentVariables(Path.Combine("%HOME%", ".corehelpers.credentials.txt"));
                Console.WriteLine($"Searching in file {filePath} for connectionstring");
                return File.ReadLines(filePath).First();
            }
        }
    }
}

