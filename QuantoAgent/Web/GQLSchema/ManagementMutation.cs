using System;
using GraphQL.Types;
using QuantoAgent.Database;
using QuantoAgent.Models;

namespace QuantoAgent.Web.GQLSchema {
    public class ManagementMutation : ObjectGraphType<object> {
        public ManagementMutation() {
            Name = "Mutation";
            Field<TokenType>("Login",
                                   arguments: new QueryArguments(
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "username" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "password" }
                                   ),
                                   resolve: ResolveLogin);
            Field<StringGraphType>("AddPartnerKey",
                          arguments: new QueryArguments(
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "serverUrl" }
                          ),
                          resolve: ResolveAddPartnerKey);
            Field<StringGraphType>("ChangePassword",
                                   arguments: new QueryArguments(
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "password" }
                                      ),
                                   resolve: ResolveChangePassword);
        }

        public object ResolveAddPartnerKey(ResolveFieldContext<object> context) {
            var ctx = (GContext)context.UserContext;
            var name = context.GetArgument<String>("name");
            var email = context.GetArgument<String>("email");
            var serverUrl = context.GetArgument<String>("serverUrl");
            // TODO
            return "OK";
        }

        public object ResolveLogin(ResolveFieldContext<object> context) {
            var ctx = (GContext)context.UserContext;
            var username = context.GetArgument<String>("username");
            var password = context.GetArgument<String>("password");

            var user = UserManager.CheckUser(username, password);

            if (user == null) {
                throw new ErrorObject {
                    ErrorCode = ErrorCodes.InvalidFieldData,
                    ErrorField = "username/password",
                    Message = "Invalid Username or Password"
                }.ToException();
            }

            return TokenManager.GenerateToken(user);
        }

        public object ResolveChangePassword(ResolveFieldContext<object> context) {
            var ctx = (GContext)context.UserContext;
            var user = ctx.User;
            var newPass = context.GetArgument<String>("password");
            if (user != null) {
                UserManager.ChangePassword(user.UserName, newPass);
                return "OK";
            }
            throw new ErrorObject {
                ErrorCode = ErrorCodes.InvalidLoginInformation,
                Message = "The specified token is either invalid or expired.",
                ErrorField = "proxyToken"
            }.ToException();
        }
    }
}
