using QuantoAgent.Database;
using QuantoAgent.Exceptions;
using QuantoAgent.Log;
using QuantoAgent.Web;

namespace QuantoAgent {
    class MainClass {
        public static void Main(string[] args) {
            Logger.GlobalEnableDebug = true;

            if (Configuration.BootstrapUser != "" && Configuration.BootstrapPass != "") {
                Logger.Log("BootstrapUser and BootstrapPass set. Creating user.");
                try {
                    UserManager.AddUser(Configuration.BootstrapUser.ToUpper(), Configuration.BootstrapUser, Configuration.BootstrapPass);
                } catch (UserAlreadyExists e) {
                    Logger.Warn($"User {Configuration.BootstrapUser} already exists. Skipping.");
                }
            }
            
            Logger.Log($"Starting {Tools.GetAppLabel()}");

            var httpServer = new Server(Configuration.HttpPort);
            httpServer.StartSync();
        }
    }
}
