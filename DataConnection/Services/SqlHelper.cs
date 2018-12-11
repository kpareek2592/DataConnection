using Apache.Phoenix;
using DataConnection.Models;
using Newtonsoft.Json;
using PhoenixSharp;
using PhoenixSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Google.Protobuf;
using pbc = Google.Protobuf.Collections;

namespace DataConnection.Services
{
    public class SqlHelper
    {
        private string ConnectionString = string.Empty;

        

        public static async void ExecuteProcedure(DataContract dataContract)
        {
            string connId = Guid.NewGuid().ToString();
            RequestOptions options = RequestOptions.GetGatewayDefaultOptions();
            var credentials = new ClusterCredentials(new Uri("jdbc:phoenix:10.16.0.206:/hbase-unsecure"), null, null);
            var client = new PhoenixClient(credentials);

            // In gateway mode, PQS requests will be https://<cluster dns name>.azurehdinsight.net/hbasephoenix<N>/
            // Requests sent to hbasephoenix0/ will be forwarded to PQS on workernode0
            options.AlternativeEndpoint = "hbasephoenix0/";
            OpenConnectionResponse openConnResponse = null;
            StatementHandle statementHandle = null;

            try
            {
                //var info = new pbc::MapField<string, string>();
                // Opening connection
                pbc::MapField<string, string> info = new pbc::MapField<string, string>();
                openConnResponse = await client.OpenConnectionRequestAsync(connId, info, options);
                // Syncing connection
                ConnectionProperties connProperties = new ConnectionProperties
                {
                    HasAutoCommit = true,
                    AutoCommit = true,
                    HasReadOnly = true,
                    ReadOnly = false,
                    TransactionIsolation = 0,
                    Catalog = "",
                    Schema = "",
                    IsDirty = true
                };
                await client.ConnectionSyncRequestAsync(connId, connProperties, options);
                var createStatementResponse = await client.CreateStatementRequestAsync(connId, options);

                string sql = "SELECT * FROM Customers";
                ExecuteResponse executeResponse = await client.PrepareAndExecuteRequestAsync(connId, sql, createStatementResponse.StatementId, long.MaxValue, int.MaxValue, options);

                pbc::RepeatedField<Row> rows = executeResponse.Results[0].FirstFrame.Rows;
                // Loop through all of the returned rows and display the first two columns
                for (int i = 0; i < rows.Count; i++)
                {
                    Row row = rows[i];
                    Console.WriteLine(row.Value[0].ScalarValue.StringValue + " " + row.Value[1].ScalarValue.StringValue);
                }

                // 100 is hard-coded on the server side as the default firstframe size
                // FetchRequestAsync is called to get any remaining rows
                Console.WriteLine("");
                Console.WriteLine($"Number of rows: {rows.Count}");

                // Fetch remaining rows, offset is not used, simply set to 0
                // When FetchResponse.Frame.Done is true, all rows were fetched
                FetchResponse fetchResponse = await client.FetchRequestAsync(connId, createStatementResponse.StatementId, 0, int.MaxValue, options);
                Console.WriteLine($"Frame row count: {fetchResponse.Frame.Rows.Count}");
                Console.WriteLine($"Fetch response is done: {fetchResponse.Frame.Done}");
                Console.WriteLine("");

                // Running query 2
                string sql2 = "select count(*) from Customers";
                ExecuteResponse countResponse = await client.PrepareAndExecuteRequestAsync(connId, sql2, createStatementResponse.StatementId, long.MaxValue, int.MaxValue, options);
                long count = countResponse.Results[0].FirstFrame.Rows[0].Value[0].ScalarValue.NumberValue;

                Console.WriteLine($"Total customer records: {count}");
                Console.WriteLine("");

                // Running query 3
                string sql3 = "select StateProvince, count(*) as Number from Customers group by StateProvince order by Number desc";
                ExecuteResponse groupByResponse = await client.PrepareAndExecuteRequestAsync(connId, sql3, createStatementResponse.StatementId, long.MaxValue, int.MaxValue, options);

                pbc::RepeatedField<Row> stateRows = groupByResponse.Results[0].FirstFrame.Rows;
                for (int i = 0; i < stateRows.Count; i++)
                {
                    Row row = stateRows[i];
                    Console.WriteLine(row.Value[0].ScalarValue.StringValue + ": " + row.Value[1].ScalarValue.NumberValue);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (statementHandle != null)
                {
                    await client.CloseStatementRequestAsync(connId, statementHandle.Id, options);
                    statementHandle = null;
                }
                if (openConnResponse != null)
                {
                    await client.CloseConnectionRequestAsync(connId, options);
                    openConnResponse = null;
                }
            }
        }

        public static string ExecuteDataAsJson(DataContract dataContract)
        {
            DataTable dt = new DataTable();
            string JSONresult;
            string MyConString = "DRIVER={org.apache.phoenix.jdbc.PhoenixDriver};" +
            "SERVER=jdbc:phoenix:10.16.0.206:/hbase-unsecure;";
            //"DATABASE=hbase-unsecure;";
            //JSONresult = JsonConvert.SerializeObject(dt);
            var query = MyConString;
            using (SqlConnection con = new SqlConnection(query))
            {
                using (SqlCommand cmd = new SqlCommand(dataContract.sqlQuery, con))
                {
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    //System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                    Dictionary<string, object> row;
                    foreach (DataRow dr in dt.Rows)
                    {
                        row = new Dictionary<string, object>();
                        //foreach (DataColumn col in dt.Columns)
                        //{
                        //    row.Add(col.ColumnName, dr[col]);
                        //}
                        //rows.Add(row);
                    }
                    JSONresult = JsonConvert.SerializeObject(dt, Formatting.Indented);
                    return JSONresult;
                }
            }
        }
    }
}