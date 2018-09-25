namespace QuantoAgent.Models {
    public struct GPGDecryptedDataReturn {
        public string FingerPrint { get; set; }
        public string Base64Data { get; set; }
        public string Filename { get; set; }
        public bool IsIntegrityProtected { get; set; }
        public bool IsIntegrityOK { get; set; }
    }
}
