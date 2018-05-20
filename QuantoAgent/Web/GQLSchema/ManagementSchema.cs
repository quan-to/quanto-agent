using System;
using GraphQL;
using GraphQL.Types;

namespace QuantoAgent.Web.GQLSchema {
    public class ManagementSchema : Schema {
        public ManagementSchema(IDependencyResolver resolver) : base(resolver) {
            Query = resolver.Resolve<ManagementQuery>();
            Mutation = resolver.Resolve<ManagementMutation>();
        }
    }
}
