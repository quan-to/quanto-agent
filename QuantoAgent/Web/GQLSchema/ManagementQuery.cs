using System;
using GraphQL.Types;
using QuantoAgent.Log;
using QuantoAgent.Models;

namespace QuantoAgent.Web.GQLSchema {
    public class ManagementQuery : ObjectGraphType<object> {
        public ManagementQuery() {
            Name = "Query";
            Field<StringGraphType>("test", resolve: ResolveTest);
        }

        public object ResolveTest(ResolveFieldContext<object> context) {
            var ctx = (GContext)context.UserContext;

            return $"{ctx.Path} - {ctx.Method}";
        }
    }
}
