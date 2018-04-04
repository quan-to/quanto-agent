using System;
using System.Net;
using System.Text;
using MimeTypes;
using QuantoAgent.Models;
using System.Net.Http;
using System.Threading.Tasks;
using QuantoAgent.Log;
using System.Reflection;

namespace QuantoAgent.Web {
    public class Proxy {
        static readonly HttpClient client = new HttpClient();

        public async Task<RestResult> ProcessRequest(string path, string method, RestRequest req) {
            if (req.Headers.GetValues("serverUrl") == null) {
                return new RestResult(new ErrorObject {
                    ErrorCode = ErrorCodes.InvalidFieldData,
                    Message = "No server URL specified",
                    ErrorField = "headers.serverUrl"
                }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.BadRequest);
            }

            var serverUrl = req.Headers.GetValues("serverUrl")[0];

            if (method != "POST") {
                return new RestResult(new ErrorObject {
                    ErrorCode = ErrorCodes.OperationNotSupported,
                    Message = "Only POST is supported for GraphQL Gateway",
                    ErrorField = "method"
                }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.BadRequest);
            }

            Logger.Debug($"Received Proxy Request for server {serverUrl}");

            var httpContent = new StringContent(req.BodyData, Encoding.UTF8, "application/json");

            httpContent.Headers.Add("X-Powered-By", Tools.GetAppLabel());

            var response = await client.PostAsync(serverUrl, httpContent);
            var result = await response.Content.ReadAsStringAsync();

            return new RestResult(result, MimeTypeMap.JSON);
        }
    }
}
