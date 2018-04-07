using System;
using GraphQL.Types;
using QuantoAgent.Log;

namespace QuantoAgent.Web.GQLSchema {
    public class ManagementQuery : ObjectGraphType<object> {
        public ManagementQuery() {
            Name = "Query";
            Field<StringGraphType>("test", resolve: context => {
                Logger.Log("HUEBR");
                return "huebr";
            });
        }
    }
}
