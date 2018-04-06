using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CoreHelpers.WindowsAzure.Storage.Table.Services
{
    internal static class DataExportServiceStringExtension {
        
        public static string AddCsvElement(this string csvLine, string propertyName, string propertyType, string propertyValue)
        {
            if (!String.IsNullOrEmpty(csvLine) && !csvLine.EndsWith(",", StringComparison.CurrentCulture))
                csvLine += ",";

            csvLine = $"{csvLine}\"{propertyName}\",\"{propertyType}\",\"{propertyValue}\"";

            return csvLine;
        }
    }

    public class DataExportService
    {
        private StorageContext storageContext { get; set; }

        public DataExportService(StorageContext storageContext)
        {
            this.storageContext = storageContext;
        }

        public async Task Export(string tableName, TextWriter targetWriter) 
        {
            // get a table reference 
            var table = storageContext.GetTableReference(tableName);

            // verify if table exists
            var existsTable = await table.ExistsAsync();
            if (!existsTable)
                throw new FileNotFoundException($"Table {tableName} does not exist");

            // prepare the table query
            var tableQuery = new TableQuery();

            TableContinuationToken tableContinuationToken = null;
            do
            {
                // do the query of the first page
                var queryResponse = await table.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken, null, null);

                // set the continouation token
                tableContinuationToken = queryResponse.ContinuationToken;

                // genenerate for every single item a valid file structure
                // "<<PropertyName>>","<<Type>>","<<Value>>",....
                foreach (var element in queryResponse.Results)
                {

                    // basic string 
                    var csvLine = String.Empty;

                    // generate the partition and rowKey
                    csvLine = csvLine.AddCsvElement("PartitionKey", "String", element.PartitionKey);
                    csvLine = csvLine.AddCsvElement("RowKey", "String", element.RowKey);

                    // visit every property
                    foreach(var propertyKvp in element.Properties) {
                        var stringRepresentation = Convert.ToString(propertyKvp.Value.PropertyAsObject);
                        csvLine = csvLine.AddCsvElement(propertyKvp.Key, propertyKvp.Value.PropertyType.ToString(), stringRepresentation);
                    }

                    // write the line into output
                    await targetWriter.WriteLineAsync(csvLine);
                }
            }
            while (tableContinuationToken != null);
        }
    }
}
