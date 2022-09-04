using System;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS020NewQueryInterface
    {
        private readonly IStorageContext _rootContext;

        public ITS020NewQueryInterface(IStorageContext context)
        {
            _rootContext = context;

        }

        [Fact]
        public async Task VerifyInPartition()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper(typeof(DemoEntityQuery));

                // create items in two different partitions                
                var modelsP1 = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {P = "P1", R = "E1", StringField = "Demo01"},
                    new DemoEntityQuery() {P = "P1", R = "E2", StringField = "Demo02"},
                };

                await scp.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(modelsP1);

                var modelsP2 = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {P = "P2", R = "E1", StringField = "Demo03"}
                };

                await scp.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(modelsP2);

                // verify that we have 3 items
                Assert.Equal(3, (await scp.Query<DemoEntityQuery>().Now()).Count());

                // verify that we have the correct two items in the partition 1
                var partition1Items = (await scp.Query<DemoEntityQuery>().InPartition("P1").Now()).ToList();
                Assert.Equal(2, partition1Items.Count());
                Assert.Equal("P1", partition1Items.First().P);
                Assert.Equal("E1", partition1Items.First().R);
                Assert.Equal("Demo01", partition1Items.First().StringField);
                Assert.Equal("P1", partition1Items.Last().P);
                Assert.Equal("E2", partition1Items.Last().R);
                Assert.Equal("Demo02", partition1Items.Last().StringField);

                // verify that we have the correct two items in the partition 2
                var partition2Items = (await scp.Query<DemoEntityQuery>().InPartition("P2").Now()).ToList();
                Assert.Single(partition2Items);
                Assert.Equal("P2", partition2Items.First().P);
                Assert.Equal("E1", partition2Items.First().R);
                Assert.Equal("Demo03", partition2Items.First().StringField);

                // cleanup
                await scp.DropTableAsync<DemoEntityQuery>();
            }
        }

        [Fact]
        public async Task VerifyGetItem()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper(typeof(DemoEntityQuery));

                // create items in two different partitions                
                var modelsP1 = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {P = "P1", R = "E1", StringField = "Demo01"},
                    new DemoEntityQuery() {P = "P1", R = "E2", StringField = "Demo02"},
                };

                await scp.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(modelsP1);

                var modelsP2 = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {P = "P2", R = "E1", StringField = "Demo03"}
                };

                await scp.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(modelsP2);

                // verify that we have 3 items
                Assert.Equal(3, (await scp.Query<DemoEntityQuery>().Now()).Count());

                // get a concrete item from partition 1
                var itemP1 = (await scp.Query<DemoEntityQuery>().InPartition("P1").GetItem("E2").Now()).ToList();
                Assert.Single(itemP1);
                Assert.Equal("P1", itemP1.First().P);
                Assert.Equal("E2", itemP1.First().R);
                Assert.Equal("Demo02", itemP1.First().StringField);

                // get a concrete item from partiiton 2
                var itemP2 = (await scp.Query<DemoEntityQuery>().InPartition("P2").GetItem("E1").Now()).ToList();
                Assert.Single(itemP2);
                Assert.Equal("P2", itemP2.First().P);
                Assert.Equal("E1", itemP2.First().R);
                Assert.Equal("Demo03", itemP2.First().StringField);

                // cleanup
                await scp.DropTableAsync<DemoEntityQuery>();
            }
        }

        [Fact]
        public async Task VerifyFilterOperations()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper(typeof(DemoEntityQuery));

                // create items in two different partitions                
                var modelsP1 = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {P = "P1", R = "E1", StringField = "Demo01", BoolField = false },
                    new DemoEntityQuery() {P = "P1", R = "E2", StringField = "Demo02", BoolField = true },
                    new DemoEntityQuery() {P = "P1", R = "E3", StringField = "Demo03", BoolField = false },
                    new DemoEntityQuery() {P = "P1", R = "E4", StringField = "Demo04", BoolField = true },
                };

                await scp.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(modelsP1);

                var result01 = await scp.Query<DemoEntityQuery>().InPartition("P1").Filter(new List<QueryFilter>()
                {
                    new QueryFilter() { FilterType = QueryFilterType.Where, Operator = QueryFilterOperator.Equal, Property = "StringField", Value = "Demo01"}
                }).Now();

                Assert.Single(result01);
                Assert.Equal("P1", result01.First().P);
                Assert.Equal("E1", result01.First().R);
                Assert.Equal("Demo01", result01.First().StringField);
                Assert.False(result01.First().BoolField);

                var result02 = await scp.Query<DemoEntityQuery>().InPartition("P1").Filter(new List<QueryFilter>()
                {
                    new QueryFilter() { FilterType = QueryFilterType.Where, Operator = QueryFilterOperator.Equal, Property = "BoolField", Value = true}
                }).Now();

                Assert.Equal(2, result02.Count());
                Assert.Equal("E2", result02.First().R);
                Assert.Equal("E4", result02.Last().R);

                // cleanup
                await scp.DropTableAsync<DemoEntityQuery>();
            }
        }

        [Fact]
        public async Task VerifyMaxItemLimitation()
        {
            using (var scp = _rootContext.CreateChildContext())
            {
                // set the tablename context
                scp.SetTableContext();

                // configure the entity mapper
                scp.AddAttributeMapper(typeof(DemoEntityQuery));

                // create items in two different partitions                
                var modelsP1 = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {P = "P1", R = "E01"},
                    new DemoEntityQuery() {P = "P1", R = "E02"},
                    new DemoEntityQuery() {P = "P1", R = "E03"},
                    new DemoEntityQuery() {P = "P1", R = "E04"},
                    new DemoEntityQuery() {P = "P1", R = "E05"},
                    new DemoEntityQuery() {P = "P1", R = "E06"},
                    new DemoEntityQuery() {P = "P1", R = "E07"},
                    new DemoEntityQuery() {P = "P1", R = "E08"},
                    new DemoEntityQuery() {P = "P1", R = "E09"},
                    new DemoEntityQuery() {P = "P1", R = "E10"},
                };

                var modelsP2 = new List<DemoEntityQuery>()
                {
                    new DemoEntityQuery() {P = "P2", R = "E01"},
                    new DemoEntityQuery() {P = "P2", R = "E02"},
                    new DemoEntityQuery() {P = "P2", R = "E03"},
                    new DemoEntityQuery() {P = "P2", R = "E04"},
                    new DemoEntityQuery() {P = "P2", R = "E05"},
                    new DemoEntityQuery() {P = "P2", R = "E06"},
                    new DemoEntityQuery() {P = "P2", R = "E07"},
                    new DemoEntityQuery() {P = "P2", R = "E08"},
                    new DemoEntityQuery() {P = "P2", R = "E09"},
                    new DemoEntityQuery() {P = "P2", R = "E10"},
                };

                await scp.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(modelsP1);
                await scp.EnableAutoCreateTable().MergeOrInsertAsync<DemoEntityQuery>(modelsP2);

                // check that we have 20 itmes                
                Assert.Equal(20, (await scp.Query<DemoEntityQuery>().Now()).Count());

                // check that the limitation to all partitions works
                Assert.Equal(2, (await scp.Query<DemoEntityQuery>().InAllPartitions().LimitTo(2).Now()).Count());

                // check that the limitation works in a dedicated partitions
                Assert.Single(await scp.Query<DemoEntityQuery>().InPartition("P2").LimitTo(1).Now());
                Assert.Equal(7, (await scp.Query<DemoEntityQuery>().InPartition("P2").LimitTo(7).Now()).Count());
                Assert.Equal(10, (await scp.Query<DemoEntityQuery>().InPartition("P2").LimitTo(10).Now()).Count());
                Assert.Equal(10, (await scp.Query<DemoEntityQuery>().InPartition("P2").LimitTo(15).Now()).Count());

                Assert.Single(await scp.Query<DemoEntityQuery>().InPartition("P1").LimitTo(1).Now());
                Assert.Equal(7, (await scp.Query<DemoEntityQuery>().InPartition("P1").LimitTo(7).Now()).Count());
                Assert.Equal(10, (await scp.Query<DemoEntityQuery>().InPartition("P1").LimitTo(10).Now()).Count());
                Assert.Equal(10, (await scp.Query<DemoEntityQuery>().InPartition("P1").LimitTo(15).Now()).Count());

                // query just a rowkey
                Assert.Equal(2, (await scp.Query<DemoEntityQuery>().InAllPartitions().GetItem("E10").LimitTo(5).Now()).Count());

                // cleanup
                await scp.DropTableAsync<DemoEntityQuery>();
            }
        }
    }
}


// TODO: Test all datatypers

/*case EdmType.String:
						if (propertyType != typeof(string))
							break;						

						property.SetOrAddValue(entity, entityProperty.StringValue, isCollection);
						break;
					case EdmType.Binary:
						if (propertyType != typeof(byte[]))						
							break;						

						property.SetOrAddValue(entity, entityProperty.BinaryValue, isCollection);
						break;
					case EdmType.Boolean:
						if (propertyType != typeof(bool) && propertyType != typeof(bool?))						
							break;						

						property.SetOrAddValue(entity, entityProperty.BooleanValue, isCollection);
						break;
					case EdmType.DateTime:
						if (propertyType == typeof(DateTime))
						{
							property.SetOrAddValue(entity, entityProperty.DateTimeOffsetValue.Value.UtcDateTime, isCollection);
						}
						else if (propertyType == typeof(DateTime?))
						{
							property.SetOrAddValue(entity, entityProperty.DateTimeOffsetValue.HasValue ? entityProperty.DateTimeOffsetValue.Value.UtcDateTime : (DateTime?)null, isCollection);
						}
						else if (propertyType == typeof(DateTimeOffset))
						{
							property.SetOrAddValue(entity, entityProperty.DateTimeOffsetValue.Value, isCollection);
						}
						else if (propertyType == typeof(DateTimeOffset?))
						{
							property.SetOrAddValue(entity, entityProperty.DateTimeOffsetValue, isCollection);
						}

						break;
					case EdmType.Double:
						if (propertyType != typeof(double) && propertyType != typeof(double?))						
							break;						

						property.SetOrAddValue(entity, entityProperty.DoubleValue, isCollection);
						break;
					case EdmType.Guid:
						if (propertyType != typeof(Guid) && propertyType != typeof(Guid?))						
							break;						

						property.SetOrAddValue(entity, entityProperty.GuidValue, isCollection);
						break;
					case EdmType.Int32:
						if (propertyType != typeof(int) && propertyType != typeof(int?) &&
							propertyType != typeof(double) && propertyType != typeof(double?))												
							break;

						if (propertyType == typeof(double) || propertyType == typeof(double?))
							property.SetOrAddValue(entity, Convert.ToDouble(entityProperty.Int32Value), isCollection);
						else												
							property.SetOrAddValue(entity, entityProperty.Int32Value, isCollection);
							
						break;
					case EdmType.Int64:
						if (propertyType != typeof(long) && propertyType != typeof(long?))						
							break;						

						property.SetOrAddValue(entity, entityProperty.Int64Value, isCollection);
						break;
				}*/