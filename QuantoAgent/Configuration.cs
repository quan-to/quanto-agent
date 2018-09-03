using System;
using System.IO;
using QuantoAgent.Database;

namespace QuantoAgent {
    public static class Configuration {
        public static string SyslogServer { get; private set; }
        public static string SyslogFacility { get; private set; }
        public static string PrivateKeyFolder { get; private set; }
        public static string KeyPrefix { get; private set; }
        public static int HttpPort { get; private set; }
        public static string MasterGPGKeyPath { get; private set; }
        public static string MasterGPGKeyPassword { get; private set; }
        public static int DefaultExpirationSeconds { get; private set; }

        static Configuration() {
            SyslogServer = ConfigurationManager.Get("SYSLOG_IP", "127.0.0.1");
            SyslogFacility = ConfigurationManager.Get("SYSLOG_FACILITY", "LOG_USER");
            PrivateKeyFolder = ConfigurationManager.Get("PRIVATE_KEY_FOLDER", "keys");
            KeyPrefix = ConfigurationManager.Get("KEY_PREFIX", "");

            var hp = ConfigurationManager.Get("HTTP_PORT", "5100");
            HttpPort = int.Parse(hp);

            MasterGPGKeyPath = ConfigurationManager.Get("MASTER_GPG_KEY_PATH", "master.key");
            MasterGPGKeyPassword = ConfigurationManager.Get("MASTER_GPG_KEY_PASSWORD", "quanto-agent");

            var des = ConfigurationManager.Get("DEFAULT_TOKEN_EXPIRATION", "3600");
            DefaultExpirationSeconds = int.Parse(des);

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            try { Directory.CreateDirectory(PrivateKeyFolder); } catch (Exception) { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
        }
    }
}
