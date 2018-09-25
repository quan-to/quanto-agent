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
                var p = (int)Environment.OSVersion.Platform;
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
                using (var stream = assembly.GetManifestResourceStream($"QuantoAgent.{filename}")) {
                    if (stream == null) {
                        throw new Exception();
                    }
                    data = new byte[stream.Length];
                    var position = 0;
                    while (position < stream.Length) {
                        var chunkSize = stream.Length - position > 4096 ? 4096 : (int)(stream.Length - position);
                        stream.Read(data, position, chunkSize);
                        position += chunkSize;
                    }
                }
            } catch (Exception) {
                Logger.Warn($"Cannot load {filename} from library.");
            }

            return data;
        }
        
        public static string Raw2AsciiArmored(byte[] b) {
            var encOut = new MemoryStream();
            var s = new ArmoredOutputStream(encOut);
            s.Write(b);
            s.Close();
            encOut.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(encOut);
            return reader.ReadToEnd();
        }

        public static string H16FP(string fingerPrint) {
            if (fingerPrint.Length < 16) {
                throw new ArgumentException("FingerPrint string has less than 16 chars!");
            }
            return fingerPrint.Substring(fingerPrint.Length - 16, 16);
        }

        public static string H8FP(string fingerPrint) {
            if (fingerPrint.Length < 8) {
                throw new ArgumentException("FingerPrint string has less than 8 chars!");
            }
            return fingerPrint.Substring(fingerPrint.Length - 8, 8);
        }

        public static string ToHexString(this byte[] ba) {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString().ToUpper();
        }

        public static Stream GenerateStreamFromByteArray(byte[] data) {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static Stream GenerateStreamFromString(string s) {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static string GPG2Quanto(string signature, string fingerPrint, HashAlgorithmTag hash) {
            var hashName = hash.ToString().ToUpper();
            var cutSig = "";

            var s = signature.Trim().Split('\n');

            for (var i = 2; i < s.Length - 1; i++) {
                cutSig += s[i];
            }

            return $"{fingerPrint}_{hashName}_{cutSig}";
        }
    }
}
