using System;
using System.Collections.Generic;
using GraphQL;

namespace QuantoAgent.Models {
    public class GraphQLBody {
        public string query { get; set; }
        public Dictionary<string, object> variables { get; set; }
        public string operationName { get; set; }


        public Inputs VariableToInputs() {
            return variables != null ? new Inputs(variables) : new Inputs();
        }
    }
}
