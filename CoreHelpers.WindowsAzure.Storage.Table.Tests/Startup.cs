using CoreHelpers.WindowsAzure.Storage.Table.Backup;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.TestEnvironments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    public class Startup
    {               
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging((lb) =>
            {
                lb.AddDebug();
            });

            services.AddTransient<ITestEnvironment, UnittestStorageEnvironment>();

            services.AddScoped<IStorageContext>((svp) =>
            {
                var env = svp.GetService<ITestEnvironment>();
                if (env == null)
                    throw new NullReferenceException();

                return new StorageContext(env.ConnectionString);
            });

            services.AddTransient<IBackupService, BackupService>();
        }
    }
}

