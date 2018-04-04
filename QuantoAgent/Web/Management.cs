using System;
using System.Net;
using MimeTypes;
using QuantoAgent.Models;

namespace QuantoAgent.Web {
    public class Management {

        public RestResult ProcessRequest(string path, string method, RestRequest req) {
            return new RestResult(new ErrorObject {
                ErrorCode = ErrorCodes.NotFound,
                Message = "Endpoint not found",
                ErrorField = "url"
            }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.NotFound);
        }
    }
}
