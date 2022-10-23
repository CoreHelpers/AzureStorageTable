using CoreHelpers.WindowsAzure.Storage.Table.Tests;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS019QueryFilter
    {
        private readonly ITestEnvironment env;

        public ITS019QueryFilter(ITestEnvironment env)
        {
            this.env = env;
        }

        [Fact]
        public async Task VerifyQueryfilter()
        {
            // Import from Blob            
            using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // set the tablename context
                storageContext.SetTableContext();

                // create the model 
                var models = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {R = "E1", StringField = "Demo01"},
                    new DemoEntityQuery() {R = "E2", StringField = "Demo02"},
                    new DemoEntityQuery() {R = "E3", StringField = "Demo02"},
                    new DemoEntityQuery() {R = "E4", StringField = "Demo03"},
                    new DemoEntityQuery() {R = "E5", StringField = "Demo03", BoolField = true}
                };

                // ensure we are using the attributes                
                storageContext.AddAttributeMapper(typeof(DemoEntityQuery));
                
                // inser the model                
                await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(models);

                // buidl a filter 
                var queryFilter = new List<QueryFilter>()
                {
                    new QueryFilter()
                    {
                        FilterType = QueryFilterType.Where, 
                        Property = nameof(DemoEntityQuery.StringField),
                        Value = "Demo03", 
                        Operator = QueryFilterOperator.Equal
                    },
                    new QueryFilter()
                    {
                        FilterType = QueryFilterType.And, 
                        Property = nameof(DemoEntityQuery.BoolField),
                        Value = true, 
                        Operator = QueryFilterOperator.Equal
                    },
                    new QueryFilter()
                    {
                        FilterType = QueryFilterType.Or, 
                        Property = nameof(DemoEntityQuery.StringField),
                        Value = "Demo02", 
                        Operator = QueryFilterOperator.Equal
                    },
                };

                // query all                
                var result = (await storageContext.QueryAsync<DemoEntityQuery>(null, queryFilter)).ToList();
                Assert.Equal(3, result.Count());
                
                result = (await storageContext.QueryAsync<DemoEntityQuery>("P1", queryFilter)).ToList();
                Assert.Equal(3, result.Count());
                
                // Clean up                 
                var all = await storageContext.QueryAsync<DemoEntityQuery>();
                await storageContext.DeleteAsync<DemoEntityQuery>(all);
                await storageContext.DropTableAsync<DemoEntityQuery>();
            }
        }

        [Fact]
        public async Task VerifyQueryfilterBoolOnly()
        {
            // Import from Blob            
            using (var storageContext = new StorageContext(env.ConnectionString))
            {
                // set the tablename context
                storageContext.SetTableContext();

                // create the model 
                var models = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {R = "E6", StringField = "Demo03"},
                    new DemoEntityQuery() {R = "E7", StringField = "Demo03", BoolField = true},
                    new DemoEntityQuery() {R = "E8", StringField = "Demo03", BoolField = false}
                };

                // ensure we are using the attributes                
                storageContext.AddAttributeMapper(typeof(DemoEntityQuery));

                // inser the model                
                await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(models);

                // build the basic filter
                var filterItem = new QueryFilter()
                {
                    FilterType = QueryFilterType.And,
                    Property = nameof(DemoEntityQuery.BoolField),
                    Value = true,
                    Operator = QueryFilterOperator.Equal
                };

                // query all elements with empty filter list
                var result = (await storageContext.QueryAsync<DemoEntityQuery>(null, new List<QueryFilter>())).ToList();
                Assert.Equal(3, result.Count());

                // query all false elements
                filterItem.Value = false;
                result = (await storageContext.QueryAsync<DemoEntityQuery>("P1", new List<QueryFilter>() { filterItem })).ToList();
                Assert.Equal(2, result.Count());

                // query all true elements
                filterItem.Value = true;
                result = (await storageContext.QueryAsync<DemoEntityQuery>("P1", new List<QueryFilter>() { filterItem })).ToList();
                Assert.Single(result);

                // Clean up                 
                var all = await storageContext.QueryAsync<DemoEntityQuery>();
                await storageContext.DeleteAsync<DemoEntityQuery>(all);
                await storageContext.DropTableAsync<DemoEntityQuery>();
            }
        }
    }
}