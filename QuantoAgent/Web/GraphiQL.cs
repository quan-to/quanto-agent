using System.IO;
using System.Net;
using System.Text;
using MimeTypes;
using QuantoAgent.Log;
using QuantoAgent.Models;

namespace QuantoAgent.Web {
    public class GraphiQL {
        public static RestResult ProcessRequest(string path, string method, RestRequest req) {
            var bundleFile = path.Replace("/", ".").Substring(1);
            if (bundleFile.Length == 0) {
                bundleFile = "index.html";
            }
            Logger.Debug($"Loading file Bundles.GraphiQL.{bundleFile}");
            var data = Tools.ReadFileFromAssembly($"Bundles.GraphiQL.{bundleFile}");
            if (data != null) {
                return new RestResult(data, MimeTypeMap.GetMimeType(Path.GetExtension(bundleFile)));
            }
            return new RestResult(new ErrorObject {
                ErrorCode = ErrorCodes.NotFound,
                Message = "Endpoint not found",
                ErrorField = "url"
            }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.NotFound);
        }
    }
}
