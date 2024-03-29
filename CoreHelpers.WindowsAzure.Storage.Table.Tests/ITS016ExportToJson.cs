﻿using System.Text;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Contracts;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Extensions;
using CoreHelpers.WindowsAzure.Storage.Table.Tests.Models;
using Xunit.DependencyInjection;

namespace CoreHelpers.WindowsAzure.Storage.Table.Tests
{
    [Startup(typeof(Startup))]
    [Collection("Sequential")]
    public class ITS016ExportToJson
    {        
        private readonly IStorageContext _rootContext;

        public ITS016ExportToJson(IStorageContext context)
        {
            _rootContext = context;

        }

        [Fact]
        public async Task VerifyExportToJson()
        {
            // Export Table
            using (var storageContext = _rootContext.CreateChildContext())
            {
                // set the tablename context
                storageContext.SetTableContext();

                // ensure we have a model registered in the correct table
                var tableName1 = $"BU".Replace("-", "");
                storageContext.AddAttributeMapper(typeof(DemoModel2), tableName1);

                // create model with data in list
                var model = new DemoModel2() { P = "1", R = "2", CreatedAt = DateTime.Parse("2023-11-10T19:09:50.065462+00:00").ToUniversalTime()};

                // inser the model                    
                await storageContext.EnableAutoCreateTable().MergeOrInsertAsync<DemoModel2>(new List<DemoModel2>() { model });

                // export
                var targetStream = new MemoryStream();
                using (var textWriter = new StreamWriter(targetStream))
                {
                    await storageContext.ExportToJsonAsync(tableName1, textWriter, (ImportExportOperation operation) => { });
                }

                // verify the targetstream
                var parsedStream = Encoding.Default.GetString(targetStream.GetBuffer()).Split("\0")[0];
                var expectedStreamValue = "[{\"RowKey\":\"2\",\"PartitionKey\":\"1\",\"Properties\":[{\"PropertyName\":\"CreatedAt\",\"PropertyType\":3,\"PropertyValue\":\"2023-11-10T19:09:50.065462+00:00\"},{\"PropertyName\":\"P\",\"PropertyType\":0,\"PropertyValue\":\"1\"},{\"PropertyName\":\"R\",\"PropertyType\":0,\"PropertyValue\":\"2\"}]}]";
                Assert.Equal(expectedStreamValue, parsedStream);

                // drop table
                await storageContext.DropTableAsync<DemoModel2>();
            }
        }        
    }
}
