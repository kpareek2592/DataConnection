using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataConnection.Models
{
    public class DataContract
    {
        [JsonProperty("driverName", Order = 1)]
        public string driverName { get; set; }

        [JsonProperty("connString", Order = 2)]
        public string connString { get; set; }

        [JsonProperty("procName", Order = 3)]
        public string procName { get; set; }

        [JsonProperty("sqlQuery", Order = 4)]
        public string sqlQuery { get; set; }
    }
}