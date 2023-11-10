using System.Text;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Newtonsoft.Json.Linq;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS017ImportFromJson
    {        
        private readonly IStorageContext _rootContext;

        public ITS017ImportFromJson(IStorageContext context)
        {
            _rootContext = context;
        }

        [Fact]
        public async Task VerifyImportFromJson()
        {
            await Task.CompletedTask;

            // Import Table
            using (var storageContext = _rootContext.CreateChildContext())
            {
                // set the tablename context
                storageContext.SetTableContext();

                // ensure we have a model registered in the correct table
                var tableName1 = $"BU".Replace("-", "");
                storageContext.AddAttributeMapper(typeof(DemoModel2), tableName1);

                // define the import data 
                var staticExportData = "[{\"RowKey\":\"2\",\"PartitionKey\":\"1\",\"Properties\":[{\"PropertyName\":\"P\",\"PropertyType\":0,\"PropertyValue\":\"1\"},{\"PropertyName\":\"R\",\"PropertyType\":0,\"PropertyValue\":\"2\"},{\"PropertyName\":\"CreatedAt\",\"PropertyType\":3,\"PropertyValue\":\"2023-01-30T22:58:40.5859427+00:00\"}]}]";
                var staticExportDataStream = new MemoryStream(Encoding.UTF8.GetBytes(staticExportData ?? ""));

                // check if we have an empty tabel before import
                Assert.Empty(await storageContext.EnableAutoCreateTable().Query<DemoModel2>().Now());

                // open the data stream
                using (var streamReader = new StreamReader(staticExportDataStream))
                {
                    // read the data
                    await storageContext.ImportFromJsonAsync(tableName1, streamReader, (ImportExportOperation) => { });
                }

                // check if we have the dara correctly imported
                Assert.Single(await storageContext.Query<DemoModel2>().Now());

                // get the data
                var data = await storageContext.Query<DemoModel2>().Now();
                Assert.Equal("1", data.First().P);
                Assert.Equal("2", data.First().R);

                var createdAtDate = DateTime.Parse("2023-01-30T22:58:40.5859427+00:00");
                var createdAtDateFromDataLoad = data.First().CreatedAt;
                
                Assert.Equal(createdAtDate.ToUniversalTime(), createdAtDateFromDataLoad);
                Assert.Equal(createdAtDate.ToUniversalTime(), createdAtDateFromDataLoad.ToUniversalTime());

                // drop table
                await storageContext.DropTableAsync<DemoModel2>();                
            }
        }        
    }
}
