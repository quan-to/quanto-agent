using System;
using QuantoAgent.Database;

namespace QuantoAgent.Models {
    public class GContext {
        public string Path { get; set; }
        public string Method { get; set; }
        public DBUser User { get; set; }
    }
}
