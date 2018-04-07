using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MimeTypes;
using QuantoAgent.Log;
using QuantoAgent.Models;

namespace QuantoAgent.Web {
    public class Server {
        public int Port { get; private set; }

        readonly HttpListener listener = new HttpListener();
        Thread listenerThread;
        bool running;
        readonly Proxy proxy;
        readonly Management management;
        protected GraphiQL graphiql;

        public Server(int port = 4040) {
            Port = port;
            listener.Prefixes.Add($"http://*:{port}/");
            listenerThread = null;
            running = false;
            proxy = new Proxy();
            management = new Management();
            graphiql = new GraphiQL();
        }

        public void Start() {
            if (listenerThread != null) {
                Logger.Log("HTTP Server", "Starting HTTP Listener");
                listener.Start();
                listenerThread = new Thread(new ThreadStart(ListenerProcessor)) {
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
            if (listenerThread != null) {
                listenerThread.Join();
                listenerThread = null;
            }
        }

        void ListenerProcessor() {
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

        void HandleRequest(object state) {
            var ctx = state as HttpListenerContext;
            try {
                // Logger.Debug("HTTP Server", $"{ctx.Request.HttpMethod} - {ctx.Request.RawUrl}");
                RestResult ret = ProcessHttpCalls(ctx.Request);
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

        RestResult ProcessHttpCalls(HttpListenerRequest request) {
            var proc = AsyncProcess(request);
            proc.Wait();
            return proc.Result;
        }

        async Task<RestResult> AsyncProcess(HttpListenerRequest request) {
            string[] ePath = request.Url.AbsolutePath.Split(new char[] { '/' }, 2, StringSplitOptions.RemoveEmptyEntries);
            RestRequest req = new RestRequest(request);
            if (ePath.Length == 0) {
                return new RestResult(new ErrorObject {
                    ErrorCode = ErrorCodes.NotFound,
                    Message = "No application specified",
                    ErrorField = "url"
                }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.NotFound);
            }
            string path = ePath.Length > 1 ? "/" + ePath[1] : "/";
            string method = request.HttpMethod;
            string app = ePath[0];

            switch (app) {
                case "graphiql": return graphiql.ProcessRequest(path, method, req);
                case "graphql": return await proxy.ProcessRequest(path, method, req);
                case "admin": return await management.ProcessRequest(path, method, req);
                default:
                    return new RestResult(new ErrorObject {
                        ErrorCode = ErrorCodes.NotFound,
                        Message = "Endpoint not found",
                        ErrorField = "url"
                    }.ToJSON(), MimeTypeMap.JSON, HttpStatusCode.NotFound);
            }
        }
    }
}
