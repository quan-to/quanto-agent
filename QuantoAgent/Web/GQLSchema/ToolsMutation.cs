using GraphQL.Types;
using QuantoAgent.Models;

namespace QuantoAgent.Web.GQLSchema {
    public class ToolsMutation  : ObjectGraphType<object> {
        public ToolsMutation() {
            Name = "Mutation";
//            Field<TokenType>("Login",
//                arguments: new QueryArguments(
//                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "username"},
//                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "password"}
//                ),
//                resolve: ResolveLogin);
        }
    }
}
