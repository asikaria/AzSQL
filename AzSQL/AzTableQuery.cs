using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;
using System.Net;

namespace AzSQL
{
    class AzTableQuery
    {

        public static IEnumerable<DynamicTableEntity> RunQuery(AzSQLParseTree parseTree, StorageCredentials creds)
        {
            // We dont handle table exceptions here - let them bubble up for the caller to see the exception
            CloudStorageAccount storageAccount = new CloudStorageAccount(creds, true);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(parseTree.tablename);
            TableQuery query = new TableQuery();
            if (parseTree.hasFilterCondition) query.FilterString = parseTree.filterCondition.ToString();
            if (!parseTree.allcolumns) query.SelectColumns = parseTree.columns;

            IEnumerable<DynamicTableEntity> results = table.ExecuteQuery(query);
            
            return results;
        }


        public static void RenderResults(IEnumerable<DynamicTableEntity> resultSet)
        {

            int count = 0;
            foreach (DynamicTableEntity row in resultSet)
            {
                Console.WriteLine("PartitionKey: {0}", row.PartitionKey);
                Console.WriteLine("RowKey: {0}", row.RowKey);
                Console.WriteLine("Timestamp: {0}", row.Timestamp);

                foreach (KeyValuePair<string, EntityProperty> field in row.Properties)
                {
                    Console.WriteLine("{0}: {1}", field.Key, EntityPropertyToString(field.Value));
                }

                Console.WriteLine();
                Console.WriteLine("------------------------");
                count++;
            }
            Console.WriteLine("Number of Rows Returned: {0}", count);
        }

        private static string EntityPropertyToString(EntityProperty e)
        {
            switch (e.PropertyType)
            {
                case EdmType.Binary: return e.BinaryValue.ToString();
                case EdmType.Boolean: return e.BooleanValue.ToString();
                case EdmType.DateTime: return e.DateTime.ToString();
                case EdmType.Double: return e.DoubleValue.ToString();
                case EdmType.Guid: return e.GuidValue.ToString();
                case EdmType.Int32: return e.Int32Value.ToString();
                case EdmType.Int64: return e.Int64Value.ToString();
                case EdmType.String: return e.StringValue;
                default: return null;

            }
        }

    }
}
