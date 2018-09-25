using QuantoAgent.Models;

namespace QuantoAgent.Exceptions {
    public class KeyNotLoadedException: ErrorObjectException {
        public KeyNotLoadedException(string fingerPrint) : base($"The key {fingerPrint} is not loaded.") {
            ErrorCode = ErrorCodes.NoDataAvailable;
            ErrorField = "key";
        }
    }
}
