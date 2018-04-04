using System;
using QuantoAgent.Log;
using QuantoAgent.Web;

namespace QuantoAgent {
    class MainClass {
        public static void Main(string[] args) {
            Logger.GlobalEnableDebug = true;
            Logger.Log("Starting QuantoAgent");


            var httpServer = new Server(Configuration.HttpPort);
            httpServer.StartSync();
        }
    }
}
