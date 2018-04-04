using System;
using System.IO;
using System.Net;
using MimeTypes;
using QuantoAgent.Log;
using QuantoAgent.Models;

namespace QuantoAgent.Web {
    public class GraphiQL {
        public RestResult ProcessRequest(string path, string method, RestRequest req) {
            string bundleFile = path.Replace("/", ".").Substring(1);
            Logger.Debug($"Loading file Bundles.GraphiQL.{bundleFile}");
            var data = Tools.ReadFileFromAssembly($"Bundles.GraphiQL.{bundleFile}");
            if (data != null) {
                return new RestResult(data, MimeTypeMap.GetMimeType(Path.GetExtension(path)));
            }
            return new RestResult(new ErrorObject {
                ErrorCode = ErrorCodes.NotFound,
                Message = "Endpoint not found",
                ErrorField = "url"
            }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.NotFound);
        }
    }
}
