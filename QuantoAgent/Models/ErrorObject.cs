using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace QuantoAgent.Models {
    public class ErrorObjectQ {
        public string errorCode { get; set; }
        public string errorField { get; set; }
        public string message { get; set; }
        public string errorData { get; set; }
        public object locations { get; set; }
    }

    public class ErrorObjectException: Exception {
        public string ErrorCode { get; set; }
        public string ErrorField { get; set; }
        public object ErrorData { get; set; }
        public object Locations { get; set; }
        public ErrorObjectException(string message) : base(message) {}

        public ErrorObjectQ ToQ() {
            return new ErrorObjectQ {
                errorCode = ErrorCode,
                errorField = ErrorField,
                message = Message,
                errorData = ErrorData != null ? JsonConvert.SerializeObject(ErrorData) : null,
                locations = Locations,
            };
        }
    }

    public class ErrorObject {

        public string ErrorCode { get; set; }
        public string ErrorField { get; set; }
        public string Message { get; set; }
        public object ErrorData { get; set; }
        public object Locations { get; set; }

        public string ToJSON() {
            return JsonConvert.SerializeObject(ToQ());
        }

        public string ToGraphQLJsonError() {
            return JsonConvert.SerializeObject(new Dictionary<string, object> {
                {"errors", new List<object> { ToQ() }}
            });
        }

        public ErrorObjectQ ToQ() {
            return new ErrorObjectQ {
                errorCode = ErrorCode,
                errorField = ErrorField,
                message = Message,
                errorData = ErrorData != null ? JsonConvert.SerializeObject(ErrorData) : null,
                locations = Locations,
            };
        }

        public Exception ToException() {
            return new ErrorObjectException(this.Message) {
                ErrorCode = ErrorCode,
                ErrorField = ErrorField,
                ErrorData = ErrorData,
                Locations = Locations,
            };
        }
    }
}
