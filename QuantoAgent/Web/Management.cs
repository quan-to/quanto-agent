using GraphQL.Execution;
using GraphQL;
using QuantoAgent.Models;

using QuantoAgent.Web.GQLSchema;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using MimeTypes;
using System.Net;
using System.Text;
using QuantoAgent.Database;

namespace QuantoAgent.Web {
    public class Management {

        readonly ISimpleContainer services;
        readonly IDocumentExecuter executer;
        readonly ManagementSchema schema;

        public Management() {
            services = new SimpleContainer();
            services.Register<ManagementQuery>();
            services.Singleton(new ManagementSchema(new FuncDependencyResolver(services.Get)));
            schema = services.Get<ManagementSchema>();

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
            if (gqlErrors?.Count > 0) {
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
            }

            return new RestResult(result);
        }
    }
}
