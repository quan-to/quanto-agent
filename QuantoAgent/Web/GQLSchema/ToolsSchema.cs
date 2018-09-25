using GraphQL;
using GraphQL.Types;

namespace QuantoAgent.Web.GQLSchema {
    public class ToolsSchema : Schema {
        public ToolsSchema(IDependencyResolver resolver) : base(resolver) {
            Query = resolver.Resolve<ToolsQuery>();
            Mutation = resolver.Resolve<ToolsMutation>();
        }
    }
}
