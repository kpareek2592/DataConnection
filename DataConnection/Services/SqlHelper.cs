using DataConnection.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DataConnection.Services
{
    public class SqlHelper
    {
        private string ConnectionString = string.Empty;

        //public SqlHelper()
        //{
        //    ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //}

        //public static List<DataRow> ExecuteProcedure(DataContract dataContract)
        //{
        //    string connString = dataContract.driverName + dataContract.connString;
        //    string sqlQuery = dataContract.sqlQuery;
        //    List<object> JSONresult;

        //    using (SqlConnection conn = new SqlConnection(dataContract.connString))
        //    {
        //        DataTable dt = new DataTable();

        //        using (var cmd = new SqlCommand(dataContract.sqlQuery, conn))
        //        {
        //            SqlDataAdapter da = new SqlDataAdapter();
        //            da.SelectCommand = cmd;

        //            da.Fill(dt);
        //            var result = dt.Rows.AsEnumerable().ToList().Select(x=>x.;

        //            List<object> list = new List<object>();
        //            list = (from DataRow row in dt.Rows 
        //                    select new list()
        //                    {
                                
        //                        UserId = row["UserId"].ToString(),
        //                        UserName = row["UserName"].ToString(),
        //                        Education = row["Education"].ToString()
        //                    })
        //            return result;
        //        }
        //    }
        //}

        public static string ExecuteDataAsJson(DataContract dataContract)
        {
            DataTable dt = new DataTable();
            string JSONresult;
            //JSONresult = JsonConvert.SerializeObject(dt);
            var query = dataContract.driverName + dataContract.connString;
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