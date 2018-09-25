using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using QuantoAgent.Exceptions;
using QuantoAgent.Log;
using QuantoAgent.Models;

namespace QuantoAgent {
    public static class GpgTools {
        private const string GpgToolsLog = "GpgTools";

        private static PgpSecretKey masterSecretKey = null;
        static PgpPrivateKey masterPrivateKey = null;

        private static string fingerPrint = null;

        static GpgTools() {
            LoadKey();
        }

        private static void LoadKey() {
            using (var s = File.OpenRead(Configuration.MasterGPGKeyPath)) {
                masterSecretKey = ReadSecretKey(s);
            }

            fingerPrint = Tools.H16FP(masterSecretKey.PublicKey.GetFingerprint().ToHexString());
            Logger.Debug(GpgToolsLog, $"Loaded key {fingerPrint}");
            if (!Configuration.ExternalKeyLoad) {
                UnlockKey(Configuration.MasterGPGKeyPassword);
            }
        }

        public static void UnlockKey(string password) {
            try {
                var dec = masterSecretKey.ExtractPrivateKey(password.ToCharArray());
                if (!TestPrivateKey(masterSecretKey.PublicKey, dec)) {
                    throw new Exception("Invalid password for master key!");
                }

                masterPrivateKey = dec;
                Logger.Log(GpgToolsLog, "Master Key Unlocked");
            } catch (Exception) {
                throw new Exception("Invalid password for master key!");
            }
        }

        private static bool TestPrivateKey(PgpPublicKey publicKey, PgpPrivateKey privateKey) {
            try {
                var testData = Encoding.ASCII.GetBytes("testdata");
                var signature = "";
                using (var ms = new MemoryStream()) {
                    var s = new ArmoredOutputStream(ms);
                    using (var bOut = new BcpgOutputStream(s)) {
                        var sGen = new PgpSignatureGenerator(publicKey.Algorithm, HashAlgorithmTag.Sha512);
                        sGen.InitSign(PgpSignature.BinaryDocument, privateKey);
                        sGen.Update(testData);
                        sGen.Generate().Encode(bOut);
                        s.Close();
                        ms.Seek(0, SeekOrigin.Begin);
                        signature = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }

                return VerifySignature(testData, signature, publicKey);
            } catch (Exception e) {
                Logger.Error(GpgToolsLog, $"Error verifing private key: {e}");
                return false;
            }
        }

        public static bool VerifySignature(byte[] data, string signature, PgpPublicKey publicKey = null) {
            PgpSignatureList p3 = null;
            using (var inputStream = PgpUtilities.GetDecoderStream(Tools.GenerateStreamFromString(signature))) {
                var pgpFact = new PgpObjectFactory(inputStream);
                var o = pgpFact.NextPgpObject();
                if (o is PgpCompressedData c1) {
                    pgpFact = new PgpObjectFactory(c1.GetDataStream());
                    p3 = (PgpSignatureList) pgpFact.NextPgpObject();
                } else {
                    p3 = (PgpSignatureList) o;
                }
            }

            var sig = p3[0];
            if (publicKey == null) {
                publicKey = masterSecretKey.PublicKey;
            }

            sig.InitVerify(publicKey);
            sig.Update(data);

            return sig.Verify();
        }

        /**
         * A simple routine that opens a key ring file and loads the first available key
         * suitable for signature generation.
         * 
         * @param input stream to read the secret key ring collection from.
         * @return a secret key.
         * @throws IOException on a problem with using the input stream.
         * @throws PGPException if there is an issue parsing the input stream.
         */
        private static PgpSecretKey ReadSecretKey(Stream input) {
            var pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(input));

            foreach (PgpSecretKeyRing keyRing in pgpSec.GetKeyRings()) {
                foreach (PgpSecretKey key in keyRing.GetSecretKeys()) {
                    if (key.IsSigningKey) {
                        return key;
                    }
                }
            }

            throw new ArgumentException("Can't find signing key in key ring.");
        }

        public static GPGDecryptedDataReturn Decrypt(string data, bool dataOnly = false) {
            var str = data;
            if (masterPrivateKey == null) {
                throw new ErrorObject {
                    ErrorCode = ErrorCodes.SealedStatus,
                    ErrorField = "gpgkey",
                    Message = "The GPG Key is currently encrypted. Please decrypt it first with Unseal"
                }.ToException();
            }

            if (dataOnly) {
                str = Tools.Raw2AsciiArmored(Convert.FromBase64String(data));
            }
           
            using (var stream = PgpUtilities.GetDecoderStream(Tools.GenerateStreamFromString(str))) {
                var pgpF = new PgpObjectFactory(stream);
                var o = pgpF.NextPgpObject();
                if (!(o is PgpEncryptedDataList enc)) {
                    enc = (PgpEncryptedDataList) pgpF.NextPgpObject();
                }

                var pbe = 
                    (
                        from PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects() 
                        let keyId = pked.KeyId.ToString("X").ToUpper() 
                        let fp = Tools.H16FP(keyId) 
                        where fp == fingerPrint select pked
                     ).FirstOrDefault();

                if (pbe == null) {
                    throw new ErrorObject {
                        ErrorCode = ErrorCodes.NotFound,
                        ErrorField = "gpgkey",
                        Message = $"Cannot find {fingerPrint} in list of encrypted payload."
                    }.ToException();
                }

                var clear = pbe.GetDataStream(masterPrivateKey);
                var plainFact = new PgpObjectFactory(clear);
                var message = plainFact.NextPgpObject();
                var outData = new GPGDecryptedDataReturn {
                    FingerPrint = fingerPrint,
                };
                
                if (message is PgpCompressedData cData) {
                    var pgpFact = new PgpObjectFactory(cData.GetDataStream());
                    message = pgpFact.NextPgpObject();
                }

                switch (message) {
                    case PgpLiteralData ld:
                        outData.Filename = ld.FileName;
                        var iss = ld.GetInputStream();
                        var buffer = new byte[16 * 1024];
                        using (var ms = new MemoryStream()) {
                            int read;
                            while ((read = iss.Read(buffer, 0, buffer.Length)) > 0) {
                                ms.Write(buffer, 0, read);
                            }

                            outData.Base64Data = Convert.ToBase64String(ms.ToArray());
                        }

                        break;
                    case PgpOnePassSignatureList _:
                        throw new PgpException("Encrypted message contains a signed message - not literal data.");
                    default:
                        throw new PgpException("Message is not a simple encrypted file - type unknown.");
                }

                outData.IsIntegrityProtected = pbe.IsIntegrityProtected();

                if (outData.IsIntegrityProtected) {
                    outData.IsIntegrityOK = pbe.Verify();
                }

                return outData;
            }
        }

        public static Task<string> SignData(byte[] data, HashAlgorithmTag hash = HashAlgorithmTag.Sha512) {
            if (masterPrivateKey == null) {
                throw new ErrorObject {
                    ErrorCode = ErrorCodes.SealedStatus,
                    ErrorField = "gpgkey",
                    Message = "The GPG Key is currently encrypted. Please decrypt it first with Unseal"
                }.ToException();
            }

            return Task.Run(() => {
                using (var ms = new MemoryStream()) {
                    var s = new ArmoredOutputStream(ms);
                    using (var bOut = new BcpgOutputStream(s)) {
                        var sGen = new PgpSignatureGenerator(masterSecretKey.PublicKey.Algorithm, hash);
                        sGen.InitSign(PgpSignature.BinaryDocument, masterPrivateKey);
                        sGen.Update(data, 0, data.Length);
                        sGen.Generate().Encode(bOut);
                        s.Close();
                        ms.Seek(0, SeekOrigin.Begin);
                        return Tools.GPG2Quanto(Encoding.UTF8.GetString(ms.ToArray()),
                            masterSecretKey.PublicKey.GetFingerprint().ToHexString(), hash);
                    }
                }
            });
        }
    }
}
