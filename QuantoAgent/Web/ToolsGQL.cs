using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using MimeTypes;
using Newtonsoft.Json;
using QuantoAgent.Database;
using QuantoAgent.Models;
using QuantoAgent.Web.GQLSchema;

namespace QuantoAgent.Web {
    public class ToolsGQL {
        
        readonly ISimpleContainer services;
        readonly IDocumentExecuter executer;
        readonly ToolsSchema schema;

        public ToolsGQL() {
            services = new SimpleContainer();
            services.Register<ToolsQuery>();
            services.Register<ToolsMutation>();
            services.Singleton(new ToolsSchema(new FuncDependencyResolver(services.Get)));
            schema = services.Get<ToolsSchema>();

            executer = new DocumentExecuter(new GraphQLDocumentBuilder(), new DocumentValidator(), new ComplexityAnalyzer());
        }

        public async Task<RestResult> ProcessRequest(string path, string method, RestRequest req) {
            DBUser user = null;
            var proxyToken = req.Headers.GetValues("proxyToken");
            if (proxyToken != null && proxyToken.Length > 0) {
                var token = proxyToken[0];
                if (TokenManager.CheckToken(token)) {
                    var username = TokenManager.GetTokenUsername(token);
                    user = UserManager.GetUser(username);
                } else {
                    return new RestResult(new ErrorObject {
                        ErrorCode = ErrorCodes.InvalidLoginInformation,
                        Message = "The specified token is either invalid or expired.",
                        ErrorField = "proxyToken"
                    }.ToGraphQLJsonError(), MimeTypeMap.JSON, HttpStatusCode.Forbidden);
                }
            }

            var context = new GContext {
                Path = path,
                Method = method,
                User = user,
            };

            var body = JsonConvert.DeserializeObject<GraphQLBody>(req.BodyData);
            var execute = await executer.ExecuteAsync(_ => {
                _.Schema = schema;
                _.Query = body.query;
                _.Root = null;
                _.Inputs = body.VariableToInputs();
                _.UserContext = context;
                _.OperationName = body.operationName;
            });

            var result = new Dictionary<string, object> {
                { "data", execute.Data }
            };

            var gqlErrors = execute.Errors;
            
            if (!(gqlErrors?.Count > 0)) return new RestResult(result);
            
            var errors = new List<ErrorObjectQ>();
            foreach (var err in gqlErrors) {
                if (err.GetBaseException().GetType() == typeof(ErrorObjectException)) {
                    var baseErr = (ErrorObjectException)err.GetBaseException();
                    errors.Add(baseErr.ToQ());
                } else {
                    errors.Add(new ErrorObject {
                        ErrorCode = ErrorCodes.GraphQLError,
                        Message = err.Message,
                        Locations = err.Locations,
                        ErrorField = "graphql",
                    }.ToQ());
                }
            }
            result.Add("errors", errors);

            return new RestResult(result);
        }
    }
}
