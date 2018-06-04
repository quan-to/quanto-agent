using System;
using System.Collections.Generic;
using System.Linq;
using QuantoAgent.Log;
using QuantoAgent.Models;

namespace QuantoAgent.Database {
    public static class TokenManager {

        static readonly List<Token> currentTokens;
        static readonly Dictionary<string, Token> valueToToken;

        static TokenManager() {
            currentTokens = new List<Token>();
            valueToToken = new Dictionary<string, Token>();
        }

        public static Token GenerateToken(DBUser user) {
            var tkn = new Token {
                UserName = user.UserName,
                UserFullName = user.Name,
                Expiration = Tools.DateTimeToUnixEpoch(DateTime.Now.AddSeconds(Configuration.DefaultExpirationSeconds))
            };

            lock (currentTokens) {
                currentTokens.Add(tkn);
                RefreshCaches();
            }

            return tkn;
        }

        public static bool CheckToken(string tokenValue) {
            var token = valueToToken.ContainsKey(tokenValue) ? valueToToken[tokenValue] : null;
            return token != null && !token.IsExpired;
        }

        public static string GetTokenUsername(string tokenValue) {
            var token = valueToToken.ContainsKey(tokenValue) ? valueToToken[tokenValue] : null;
            return token?.UserName;
        }

        static void RefreshCaches() {
            valueToToken.Clear();
            currentTokens.ForEach(tkn => {
                valueToToken.Add(tkn.Value, tkn);
            });
        }

        public static void CleanExpiredTokens() {
            Logger.Log("TokenManager", "Cleaning expired tokens");
            lock(currentTokens) {
                var tksToRemove = currentTokens.Where(tkn => tkn.IsExpired).ToList();
                tksToRemove.ForEach(tkn => { currentTokens.Remove(tkn); });
                Logger.Log("TokenManager", $"{tksToRemove.Count} expired tokens removed.");
                RefreshCaches();
            }
        }
    }
}
