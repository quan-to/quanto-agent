using System;
using System.IO;
using System.Reflection;
using QuantoAgent.Log;

namespace QuantoAgent {
    public static class Tools {
        public static bool IsLinux {
            get {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        internal static byte[] ReadFileFromAssembly(string filename) {
            byte[] data = null;
            var assembly = Assembly.GetExecutingAssembly();
            try {
                using (Stream stream = assembly.GetManifestResourceStream($"QuantoAgent.{filename}")) {
                    data = new byte[stream.Length];
                    int position = 0;
                    while (position < stream.Length) {
                        int chunkSize = stream.Length - position > 4096 ? 4096 : (int)(stream.Length - position);
                        stream.Read(data, position, chunkSize);
                        position += chunkSize;
                    }
                }
            } catch (Exception) {
                Logger.Warn($"Cannot load {filename} from library.");
            }

            return data;
        }
    }
}
