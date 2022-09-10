using System;
using CoreHelpers.WindowsAzure.Storage.Table.Backup;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS21VerifyBackup
    {
        private readonly IStorageContext _rootContext;
        private readonly IBackupService _backupService;
        private readonly ITestEnvironment _testEnvironment;

        public ITS21VerifyBackup(IStorageContext context, IBackupService backupService, ITestEnvironment testEnvironment)
        {
            _rootContext = context;
            _backupService = backupService;
            _testEnvironment = testEnvironment;
        }

        [Fact]
        public async Task CreateAndVerifyBackup()
        {
            var containerName = $"bck{Guid.NewGuid().ToString()}".Replace("_", "");
            var targetPath = $"CreateAndVerifyBackup/{Guid.NewGuid()}";

            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();                

                // configure the entity mapper
                scp.AddAttributeMapper(typeof(DemoEntityQuery), "BackupDemoEntityQuery");

                // verify that we have no items
                Assert.Empty((await scp.EnableAutoCreateTable().Query<DemoEntityQuery>().Now()));

                // create items in two different partitions                
                var modelsP1 = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {P = "P1", R = "E1", StringField = "Demo01"},
                    new DemoEntityQuery() {P = "P1", R = "E2", StringField = "Demo02"},
                };

                await scp.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(modelsP1);

                using (var backupContext = await _backupService.OpenBackupContext(_testEnvironment.ConnectionString, containerName, targetPath, "Backup"))
                {
                    await backupContext.Backup(scp, null, true);
                }

                await scp.DropTableAsync<DemoEntityQuery>();
            }

            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper(typeof(DemoEntityQuery), "BackupDemoEntityQuery");

                var itemsBeforeRestore= await scp.EnableAutoCreateTable().Query<DemoEntityQuery>().Now();
                Assert.Empty(itemsBeforeRestore);

                // verify that we have no items
                Assert.Empty((await scp.EnableAutoCreateTable().Query<DemoEntityQuery>().Now()));

                // restore 
                using (var restoreContext = await _backupService.OpenRestorContext(_testEnvironment.ConnectionString, containerName, targetPath, "Backup"))
                {
                    await restoreContext.Restore(scp, null);                    
                }

                // verify if we have the values
                var items = await scp.EnableAutoCreateTable().Query<DemoEntityQuery>().Now();
                Assert.Equal(2, items.Count());
            }
        }
    }
}

