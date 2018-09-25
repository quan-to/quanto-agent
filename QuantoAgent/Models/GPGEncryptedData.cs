using System;

namespace QuantoAgent.Models {
    public struct GPGEncryptData {
        public string FingerPrint { get; set; }
        public string Base64Data { get; set; }
        public string Filename { get; set; }
        public bool DataOnly { get; set; }
    }
}
