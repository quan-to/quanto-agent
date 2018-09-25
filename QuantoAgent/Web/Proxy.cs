using System;
using System.Net;
using System.Text;
using MimeTypes;
using QuantoAgent.Models;
using System.Net.Http;
using System.Threading.Tasks;
using QuantoAgent.Log;
using System.Reflection;
using QuantoAgent.Database;

namespace QuantoAgent.Web {
    public static class Proxy {
        private static readonly HttpClient Client = new HttpClient();

        public static async Task<RestResult> ProcessRequest(string path, string method, RestRequest req) {
            if (req.Headers.GetValues("serverUrl") == null) {
                return new RestResult(new ErrorObject {
                    ErrorCode = ErrorCodes.InvalidFieldData,
                    Message = "No server URL specified",
                    ErrorField = "headers.serverUrl"
                }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.BadRequest);
            }

            var serverUrl = req.Headers.GetValues("serverUrl")?[0];
            var proxyToken = req.Headers.GetValues("proxyToken");

            if (method != "POST") {
                return new RestResult(new ErrorObject {
                    ErrorCode = ErrorCodes.OperationNotSupported,
                    Message = "Only POST is supported for GraphQL Gateway",
                    ErrorField = "method"
                }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.BadRequest);
            }

            Logger.Debug($"Received Proxy Request for server {serverUrl}");

            var httpContent = new StringContent(req.BodyData, Encoding.UTF8, "application/json");

            try {
                if (proxyToken != null && proxyToken.Length > 0) {
                    var token = proxyToken[0];
                    if (TokenManager.CheckToken(token)) {
                        var sigTask = GpgTools.SignData(Encoding.UTF8.GetBytes(req.BodyData));
                        sigTask.Wait();
                        httpContent.Headers.Add("signature", sigTask.Result);
                    } else {
                        return new RestResult(new ErrorObject {
                            ErrorCode = ErrorCodes.InvalidLoginInformation,
                            Message = "The specified token is either invalid or expired.",
                            ErrorField = "proxyToken"
                        }.ToGraphQLJsonError(), MimeTypeMap.JSON, HttpStatusCode.Forbidden);
                    }
                }
            } catch (Exception) {
                return new RestResult(new ErrorObject {
                    ErrorCode = ErrorCodes.SealedStatus,
                    ErrorField = "gpgkey",
                    Message = "The GPG Key is currently encrypted. Please decrypt it first with Unseal"
                }.ToGraphQLJsonError(), MimeTypeMap.JSON, HttpStatusCode.Forbidden);
            }

            httpContent.Headers.Add("X-Powered-By", Tools.GetAppLabel());

            var response = await Client.PostAsync(serverUrl, httpContent);
            var result = await response.Content.ReadAsStringAsync();

            return new RestResult(result, MimeTypeMap.JSON);
        }
    }
}
