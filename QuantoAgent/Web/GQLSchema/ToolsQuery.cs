using GraphQL.Types;
using QuantoAgent.Models;

namespace QuantoAgent.Web.GQLSchema {
    public class ToolsQuery : ObjectGraphType<object> {
        public ToolsQuery() {
            Name = "Query";
            Field<StringGraphType>(
                "Decrypt", 
                resolve: ResolveDecrypt,
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "gpgData"}
                ));
        }

        private static object ResolveDecrypt(ResolveFieldContext<object> context) {
            var gpgData = context.GetArgument<string>("gpgData");
            if (gpgData == null) {
                throw new ErrorObject {
                    ErrorCode = ErrorCodes.InvalidFieldData,
                    Message = "Invalid GPG Payload",
                    ErrorField = "gpgData",
                }.ToException();
            }

            return GpgTools.Decrypt(gpgData);
        }
    }
}
