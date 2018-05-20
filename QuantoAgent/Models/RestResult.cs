﻿using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace QuantoAgent.Models {
    [Serializable]
    public class RestResult {
        HttpStatusCode statusCode;
        string contentType;
        byte[] result;

        public HttpStatusCode StatusCode {
            get { return statusCode; }
            set { statusCode = value; }
        }

        public string ContentType {
            get { return contentType; }
            set { contentType = value; }
        }

        public byte[] Result {
            get { return result; }
            set { result = value; }
        }

        public RestResult() {
            statusCode = HttpStatusCode.OK;
            contentType = MimeTypes.MimeTypeMap.GetMimeType(".txt");
            result = new byte[0];
        }

        public RestResult(string result) : this() {
            this.result = Encoding.UTF8.GetBytes(result);
        }

        public RestResult(string result, string contentType) : this() {
            this.result = Encoding.UTF8.GetBytes(result);
            this.contentType = contentType;
        }

        public RestResult(object result) : this() {
            string data = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            this.result = Encoding.UTF8.GetBytes(data);
            this.contentType = MimeTypes.MimeTypeMap.JSON;
        }

        public RestResult(byte[] result, string contentType) : this() {
            this.result = result;
            this.contentType = contentType;
        }
        public RestResult(string result, string contentType, HttpStatusCode statusCode) {
            this.result = Encoding.UTF8.GetBytes(result);
            this.contentType = contentType;
            this.statusCode = statusCode;
        }

        public RestResult(byte[] result, string contentType, HttpStatusCode statusCode) {
            this.result = result;
            this.contentType = contentType;
            this.statusCode = statusCode;
        }
    }
}
