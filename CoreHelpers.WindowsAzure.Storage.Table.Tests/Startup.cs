using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.TestEnvironments;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    public class Startup
    {               
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ITestEnvironment, UnittestStorageEnvironment>();

            services.AddScoped<IStorageContext>((svp) =>
            {
                var env = svp.GetService<ITestEnvironment>();
                return new StorageContext(env.ConnectionString);
            });
        }
    }
}

