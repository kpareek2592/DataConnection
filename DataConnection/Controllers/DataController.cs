using DataConnection.Models;
using DataConnection.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DataConnection.Controllers
{
    public class DataController : ApiController
    {
        //[HttpGet]
        //[Route("api/data")]
        //public HttpResponseMessage Get(DataContract dataContract)
        //{
        //    var result = SqlHelper.ExecuteProcedure(dataContract);
        //    return Request.CreateResponse(HttpStatusCode.OK, result);
        //}

        [HttpGet]
        [Route("api/data1")]
        public HttpResponseMessage GetData(DataContract dataContract)
        {
            var result = SqlHelper.ExecuteDataAsJson(dataContract);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
