using System;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS030SharedTable
    {
        private readonly IStorageContext _rootContext;

        public ITS030SharedTable(IStorageContext context)
        {
            _rootContext = context;

        }


        [Fact]
        public async Task VerifyGetItem()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper(typeof(MultipleModelsBase));


                var model1 = new MultipleModels1() { P = "P1", Contact = "C1", Model1Field = "Model1Field" };
                var model2 = new MultipleModels2() { P = "P1", Contact = "C2", Model2Field = "Model2Field" };


                scp.EnableAutoCreateTable();

                await scp.MergeOrInsertAsync<MultipleModelsBase>(new[] {model1});
                await scp.MergeOrInsertAsync<MultipleModelsBase>(new[] {model2});


                var result1 = await scp.QueryAsync<MultipleModelsBase>("P1", "C1");
                Assert.Equivalent(model1, result1, true);
                Assert.IsType<MultipleModels1>(result1);

                var result2 = await scp.QueryAsync<MultipleModelsBase>("P1", "C2");
                Assert.Equivalent(model2, result2, true);
                Assert.IsType<MultipleModels2>(result2);

                // cleanup
                await scp.DropTableAsync<MultipleModelsBase>();
            }
        }

    }
}
