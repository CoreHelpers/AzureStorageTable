using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.TestEnvironments;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    public class Startup
    {               
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ITestEnvironment, CredentialsFilesEnvironment>();
        }
    }
}

