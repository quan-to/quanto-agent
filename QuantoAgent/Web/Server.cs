using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MimeTypes;
using QuantoAgent.Log;
using QuantoAgent.Models;

namespace QuantoAgent.Web {
    public class Server {
        private int Port { get; set; }

        readonly HttpListener listener = new HttpListener();
        Thread listenerThread;
        bool running;
        readonly Management management;
        readonly ToolsGQL toolsGql;

        public Server(int port = 4040) {
            Port = port;
            listener.Prefixes.Add($"http://*:{port}/");
            listenerThread = null;
            running = false;
            management = new Management();
            toolsGql = new ToolsGQL();
        }

        public void Start() {
            if (listenerThread != null) {
                Logger.Log("HTTP Server", "Starting HTTP Listener");
                listener.Start();
                listenerThread = new Thread(ListenerProcessor) {
                    IsBackground = true
                };
                running = true;
                listenerThread.Start();
            } else {
                Logger.Error("HTTP Server", "HTTP Thread already started!");
            }
        }

        public void StartSync() {
            listener.Start();
            running = true;
            ListenerProcessor();
        }

        public void Stop() {
            running = false;
            if (listenerThread == null) return;
            listenerThread.Join();
            listenerThread = null;
        }

        private void ListenerProcessor() {
            while (running) {
                try {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
                } catch (Exception e) {
                    Logger.Error($"Error handling HTTP Request: {e}");
                }
            }
            listener.Stop();
        }

        private void HandleRequest(object state) {
            var ctx = state as HttpListenerContext;
            try {
                // Logger.Debug("HTTP Server", $"{ctx.Request.HttpMethod} - {ctx.Request.RawUrl}");
                var ret = ProcessHttpCalls(ctx.Request);
                ctx.Response.ContentType = ret.ContentType;
                ctx.Response.StatusCode = (int)ret.StatusCode;
                ctx.Response.ContentLength64 = ret.Result.Length;
                ctx.Response.OutputStream.Write(ret.Result, 0, ret.Result.Length);
            } catch (Exception e) {
                Logger.Error("HTTP Server", $"Error processing HTTP Request: {e}");
            } finally {
                ctx.Response.OutputStream.Close();
            }
        }

        private RestResult ProcessHttpCalls(HttpListenerRequest request) {
            var proc = AsyncProcess(request);
            proc.Wait();
            return proc.Result;
        }

        private async Task<RestResult> AsyncProcess(HttpListenerRequest request) {
            var ePath = request.Url.AbsolutePath.Split(new [] { '/' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var req = new RestRequest(request);
            if (ePath.Length == 0) {
                return new RestResult(new ErrorObject {
                    ErrorCode = ErrorCodes.NotFound,
                    Message = "No application specified",
                    ErrorField = "url"
                }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.NotFound);
            }
            var path = ePath.Length > 1 ? "/" + ePath[1] : "/";
            var method = request.HttpMethod;
            var app = ePath[0];

            switch (app) {
                case "graphiql": return GraphiQL.ProcessRequest(path, method, req);
                case "graphql": return await Proxy.ProcessRequest(path, method, req);
                case "admin": return await management.ProcessRequest(path, method, req);
                case "tools": return await toolsGql.ProcessRequest(path, method, req);
                default:
                    return new RestResult(new ErrorObject {
                        ErrorCode = ErrorCodes.NotFound,
                        Message = "Endpoint not found",
                        ErrorField = "url"
                    }.ToGraphQLJsonError(), MimeTypeMap.JSON, HttpStatusCode.NotFound);
            }
        }
    }
}
