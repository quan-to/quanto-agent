using QuantoAgent.Log;
using QuantoAgent.Web;

namespace QuantoAgent {
    class MainClass {
        public static void Main(string[] args) {
            Logger.GlobalEnableDebug = true;
            Logger.Log($"Starting {Tools.GetAppLabel()}");


            var httpServer = new Server(Configuration.HttpPort);
            httpServer.StartSync();
        }
    }
}
