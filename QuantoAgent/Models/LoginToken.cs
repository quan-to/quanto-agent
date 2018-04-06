using System;
namespace QuantoAgent.Models {
    public class LoginToken {
        public string Id { get; set; }
        public DateTime Expiration { get; set; }
        public string Name { get; set; }
        public DateTime LoggedSince { get; set; }
    }
}
