using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSBlogCSharp.Database;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using AWSBlogModel;
using Amazon.XRay.Recorder.Core;

[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSBlogCSharp
{
    class BlogPostsGetAll
    {
        Dictionary<string,string> secrets;
        BlogPostContext bpc;

        public BlogPostsGetAll() {
            secrets = GetSecrets.GetSecretsDictionary();
            string connstr = GetSecrets.GetSecretConnectionString();
            Console.WriteLine($"Connection string:: {connstr}");
            bpc = GetConnectionString.GetContext(connstr);
            AWSXRayRecorder.InitializeInstance();
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request, ILambdaContext context)
        {

            string user = request.PathParameters["user"];

            context.Logger.LogLine($"USER PARAMETER IS {user}");

            // ToDo: Not sure this is the best way to do this - probalby ought to use a DI framework.
            context.Logger.LogLine("Get ALL Request\n");
            context.Logger.LogLine($"Here's the identity id = {context.Identity.IdentityId}");
            context.Logger.LogLine($"Here's the pool id = {context.Identity.IdentityPoolId}");
            
            if (request.RequestContext != null && request.RequestContext.Identity != null)
            {
                var atu = request.RequestContext.Authorizer;
                if (atu != null)
                {
                    foreach (var (k, v) in atu)
                    {
                        context.Logger.LogLine($"Authorizer obj {k} value {v}");
                    }
                }
                else
                {
                    context.Logger.LogLine("Atu is null - ie the Authorizer in request context");
                }
                context.Logger.LogLine($"USER::{request.RequestContext.Identity.User}"); // AROA3LOFKCR5TBKWJMMYN:CognitoIdentityCredentials
                context.Logger.LogLine($"USERARN::{request.RequestContext.Identity.UserArn}"); // arn:aws:sts::780487234683:assumed-role/InvokeRoleAll/CognitoIdentityCredentials
                context.Logger.LogLine($"COGNITOIDENTITY::{request.RequestContext.Identity.CognitoIdentityId}"); // eu-west-2:eda6f0b8-ec5f-43df-915e-f164e4d83ee9
                context.Logger.LogLine($"IDENTITYPOOLID::{request.RequestContext.Identity.CognitoIdentityPoolId}"); // eu-west-2:5b2ed568-fa52-4376-b719-b74bcc698b92
                context.Logger.LogLine($"ACCOUNTID::{request.RequestContext.AccountId}");
                context.Logger.LogLine($"CALLER::{request.RequestContext.Identity.Caller}");

            }

            context.Logger.LogLine("Headers::");
            if (request.Headers != null)
            {
                foreach(var h in request.Headers)
                {
                    context.Logger.LogLine($"{h.Key} :: {h.Value}");
                }
            }
            context.Logger.LogLine("Headers DONE!");

            //bool statusClause = false;
            //bool statusStr = false;
            //if (request.QueryStringParameters.ContainsKey("status")) {
            //    if (!Boolean.TryParse(request.PathParameters["status"], out statusStr))
            //        return new APIGatewayProxyResponse {
            //            StatusCode = (int) HttpStatusCode.BadRequest,
            //            Body = "{ \"error\":\"Bad status parameter\"}"
            //        };
            //statusClause = true;
            //}
            //IOrderedQueryable<DBBlogPost> versions;

            //if (statusClause)

            //var versions2 = (from blog in bpc.BlogPost
            //                group blog by blog.Id into g
            //                select new
            //                {
            //                    Id = g.Key,
            //                    Version = (from t in g select t.Version).Max()
            //                }).ToList();

            var versions = (from blog in bpc.BlogPost
                            where blog.User == user
                            group blog by blog.Id into g
                            select new { Id = g.Key, Version = g.Max(x => x.Version) });

            Dictionary<int, int> versionslist = new Dictionary<int, int>();
            foreach (var x in versions) versionslist.Add(x.Id, x.Version);

            foreach (var (k, v) in versionslist) context.Logger.LogLine($"For id {k} we want version {v}");

            var data = (from blog in bpc.BlogPost
                        where blog.User == user
                        select new { Id = blog.Id, Version = blog.Version, Title = blog.Title, File = blog.File, Date = blog.Date })
                        .ToList()
                        .Where(blog => versionslist.Keys.Contains(blog.Id) && versionslist[blog.Id] == blog.Version);

            List< BlogPostURL > all = new List<BlogPostURL>();
//            foreach (var x in versions)
//            {
//                all.Add(new BlogPostURL($"/blog/{x.Id}?version={x.Version}"));
//            }
            foreach (var x in data)
            {
                all.Add(new BlogPostURL
                {
                    URL = $"/blog/{x.Id}?version={x.Version}",
                    Title = x.Title,
                    Id = x.Id,
                    Version = x.Version,
                    Date = x.Date
                });
            }
            //from blog in bpc.BlogPost where (blog.Status == statusStr) orderby blog.Version descending select blog;
            //else
            //    versions = from blog in bpc.BlogPost group blog by blog.Id into g 
            //                select new BlogPostModel { Id = blog.Id, 
            //                                           Version = (from t in g select t.Version).Max()};
            //from blog in bpc.BlogPost orderby blog.Version descending select blog;

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = System.Text.Json.JsonSerializer.Serialize<List<BlogPostURL>>(all),
                Headers = new Dictionary<string, string> {   { "Content-Type", "application/json" } 
                                                           , { "Access-Control-Allow-Origin" , "*" }
                                                         }
            };
        }
    }
}
