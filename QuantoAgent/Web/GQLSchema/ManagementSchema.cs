using System;
using GraphQL;
using GraphQL.Types;

namespace QuantoAgent.Web.GQLSchema {
    public class ManagementSchema : Schema {
        public ManagementSchema(Func<Type, IGraphType> resolver) : base(resolver) {
            Query = resolver<ManagementQuery>();
            Mutation = resolver.Resolve<ManagementMutation>();
        }
    }
}
