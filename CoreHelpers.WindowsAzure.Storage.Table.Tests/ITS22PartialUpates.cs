using System;
using CoreHelpers.WindowsAzure.Storage.Table.Backup;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    public class ITS22PartialUpates
    {
        private readonly IStorageContext _rootContext;

        public ITS22PartialUpates(IStorageContext context)
        {
            _rootContext = context;
        }

        [Fact]
        public async Task CreateAndVerifyBackup()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper<PartialUpdateModel>();

                // verify we don't have any value
                Assert.Empty(await scp.EnableAutoCreateTable().Query<PartialUpdateModel>().InAllPartitions().Now());


                // insert the value with just on optional integer
                await scp.MergeOrInsertAsync<PartialUpdateModel>(new PartialUpdateModel() { CustomerId = "C01", MeterId = "M01", Value01 = 1 });

                var result = (await scp.EnableAutoCreateTable().Query<PartialUpdateModel>().Now()).First();
                Assert.True(result.Value01.HasValue);
                Assert.Equal(result.Value01, 1);
                Assert.False(result.Value02.HasValue);
                Assert.False(result.Value03.HasValue);

                // add the second update with just Value02 and ensure that Value01 stays untouched
                await scp.MergeOrInsertAsync<PartialUpdateModel>(new PartialUpdateModel() { CustomerId = "C01", MeterId = "M01", Value02 = 2 });

                result = (await scp.EnableAutoCreateTable().Query<PartialUpdateModel>().Now()).First();
                Assert.True(result.Value01.HasValue);
                Assert.Equal(result.Value01, 1);
                Assert.True(result.Value02.HasValue);
                Assert.Equal(result.Value02, 2);
                Assert.False(result.Value03.HasValue);

                // replace the model with Value 03
                await scp.InsertOrReplaceAsync<PartialUpdateModel>(new PartialUpdateModel() { CustomerId = "C01", MeterId = "M01", Value03 = 3 });

                result = (await scp.EnableAutoCreateTable().Query<PartialUpdateModel>().Now()).First();
                Assert.False(result.Value01.HasValue);                
                Assert.False(result.Value02.HasValue);                
                Assert.True(result.Value03.HasValue);
                Assert.Equal(result.Value03, 3);

                // clean up
                scp.DropTable<PartialUpdateModel>();
            }
        }
    }
}

