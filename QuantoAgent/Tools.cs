using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Org.BouncyCastle.Bcpg;
using QuantoAgent.Log;

namespace QuantoAgent {
    public static class Tools {
        public static bool IsLinux {
            get {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static DateTime UnixEpochToDateTime(ulong epochTime) {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epochTime).ToLocalTime();
        }

        public static ulong DateTimeToUnixEpoch(DateTime dtime) {
            return (ulong) (dtime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        internal static string GetAppLabel() {
            var asm = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            return $"QuantoAgent {fvi.FileVersion}";
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

        public static String H16FP(string fingerPrint) {
            if (fingerPrint.Length < 16) {
                throw new ArgumentException("FingerPrint string has less than 16 chars!");
            }
            return fingerPrint.Substring(fingerPrint.Length - 16, 16);
        }

        public static String H8FP(string fingerPrint) {
            if (fingerPrint.Length < 8) {
                throw new ArgumentException("FingerPrint string has less than 8 chars!");
            }
            return fingerPrint.Substring(fingerPrint.Length - 8, 8);
        }

        public static string ToHexString(this byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString().ToUpper();
        }

        public static Stream GenerateStreamFromByteArray(byte[] data) {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static Stream GenerateStreamFromString(string s) {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static String GPG2Quanto(string signature, string fingerPrint, HashAlgorithmTag hash) {
            string hashName = hash.ToString().ToUpper();
            string cutSig = "";

            string[] s = signature.Trim().Split('\n');

            for (int i = 2; i < s.Length - 1; i++) {
                cutSig += s[i];
            }

            return $"{fingerPrint}_{hashName}_{cutSig}";
        }
    }
}
