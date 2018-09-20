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
                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "username"},
                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "password"}
                ),
                resolve: ResolveLogin);
            Field<StringGraphType>("AddPartnerKey",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "name"},
                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "email"},
                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "serverUrl"}
                ),
                resolve: ResolveAddPartnerKey);
            Field<StringGraphType>("ChangePassword",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "password"}
                ),
                resolve: ResolveChangePassword);
            Field<StringGraphType>("Unseal",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "password"}
                ),
                resolve: ResolveUnseal);

        }

        public object ResolveUnseal(ResolveFieldContext<object> context) {
            var password = context.GetArgument<string>("password");
            try {
                GpgTools.UnlockKey(password);
            } catch (Exception e) {
                throw new ErrorObject {
                    ErrorCode = ErrorCodes.InvalidLoginInformation,
                    Message = "Invalid key password",
                    ErrorField = "password",
//                    ErrorData = e
                }.ToException();
            }

            return "OK";
        }

        public object ResolveAddPartnerKey(ResolveFieldContext<object> context) {
            var ctx = (GContext) context.UserContext;
            var name = context.GetArgument<string>("name");
            var email = context.GetArgument<string>("email");
            var serverUrl = context.GetArgument<string>("serverUrl");
            throw new ErrorObject {
                ErrorCode = ErrorCodes.NotImplemented,
                Message = "The specified call is not implemented",
                ErrorField = "server"
            }.ToException();
            return "OK";
        }

        public object ResolveLogin(ResolveFieldContext<object> context) {
            var ctx = (GContext) context.UserContext;
            var username = context.GetArgument<string>("username");
            var password = context.GetArgument<string>("password");

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
            var ctx = (GContext) context.UserContext;
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
