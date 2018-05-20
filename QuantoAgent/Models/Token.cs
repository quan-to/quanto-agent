using System;
using GraphQL.Types;

namespace QuantoAgent.Models {
    public class Token {
        public String Value { get; set; }
        public String UserName { get; set; }
        public String UserFullName { get; set; }
        public ulong Expiration { get; set; }
        public String ExpirationDateTimeISO {
            get {
                return Tools.UnixEpochToDateTime(Expiration).ToString("o");
            }
        }

        public bool IsExpired { 
            get {
                return Tools.DateTimeToUnixEpoch(DateTime.Now) > Expiration;        
            }
        }

        public Token() {
            Value = Guid.NewGuid().ToString();
        }
    }

    public class TokenType: ObjectGraphType<Token> {
        public TokenType() {
            Name = "Token";
            Field<StringGraphType>(name: "Value", description: "Token Value. Use this for all authenticated calls");
            Field<StringGraphType>(name: "UserName", description: "Name of the user this token belongs");
            Field<FloatGraphType>(name: "Expiration", description: "Unix Epoch Timestamp when this token expires");
            Field<StringGraphType>(name: "ExpirationDateTimeISO", description: "ISO DateTime when this token expires");
            Field<StringGraphType>(name: "UserFullName", description: "Full name of the user");
        }
    }
}
