using System;
using CoreHelpers.WindowsAzure.Storage.Table.Backup;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    public class ITS23PartialDirectoryUpates
    {
        private readonly IStorageContext _rootContext;

        public ITS23PartialDirectoryUpates(IStorageContext context)
        {
            _rootContext = context;
        }

        [Fact]
        public async Task VerifyDirectoryUpdate()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper<PartialDirectoryUpdateModel>();

                // verify we don't have any value
                Assert.Empty(await scp.EnableAutoCreateTable().Query<PartialDirectoryUpdateModel>().InAllPartitions().Now());


                // insert the model with no dictionary
                await scp.MergeOrInsertAsync<PartialDirectoryUpdateModel>(new PartialDirectoryUpdateModel() { CustomerId = "C01", MeterId = "M01" });

                var result = (await scp.EnableAutoCreateTable().Query<PartialDirectoryUpdateModel>().Now()).First();
                Assert.Empty(result.Costs.Values);

                // insert the model with a 2 value dictionary
                await scp.MergeOrInsertAsync<PartialDirectoryUpdateModel>(new PartialDirectoryUpdateModel() { CustomerId = "C01", MeterId = "M01", Costs = new Dictionary<int, double>() {
                    { 1, 11.1 },
                    { 5, 55.5 }
                } });

                result = (await scp.EnableAutoCreateTable().Query<PartialDirectoryUpdateModel>().Now()).First();
                Assert.Equal(2, result.Costs.Values.Count);
                Assert.Equal(11.1, result.Costs[1]);
                Assert.Equal(55.5, result.Costs[5]);
                Assert.False(result.Costs.ContainsKey(0));
                Assert.False(result.Costs.ContainsKey(2));
                Assert.False(result.Costs.ContainsKey(3));
                Assert.False(result.Costs.ContainsKey(4));

                // add another value 
                await scp.MergeOrInsertAsync<PartialDirectoryUpdateModel>(new PartialDirectoryUpdateModel()
                {
                    CustomerId = "C01",
                    MeterId = "M01",
                    Costs = new Dictionary<int, double>() {
                    { 2, 22.2 }                    
                }
                });

                result = (await scp.EnableAutoCreateTable().Query<PartialDirectoryUpdateModel>().Now()).First();
                Assert.Equal(3, result.Costs.Values.Count);
                Assert.Equal(11.1, result.Costs[1]);
                Assert.Equal(22.2, result.Costs[2]);
                Assert.Equal(55.5, result.Costs[5]);
                Assert.False(result.Costs.ContainsKey(0));                
                Assert.False(result.Costs.ContainsKey(3));
                Assert.False(result.Costs.ContainsKey(4));

                // override
                await scp.InsertOrReplaceAsync<PartialDirectoryUpdateModel>(new PartialDirectoryUpdateModel() { CustomerId = "C01", MeterId = "M01" });
                result = (await scp.EnableAutoCreateTable().Query<PartialDirectoryUpdateModel>().Now()).First();
                Assert.Empty(result.Costs.Values);

                // clean up
                scp.DropTable<PartialDirectoryUpdateModel>();
            }
        }
    }
}

