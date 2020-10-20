using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using AWSBlogCSharp.Database;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Amazon.S3;
using System.Threading.Tasks;
using System.IO;
using AWSBlogCSharp.Model;

[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSBlogCSharp
{
    class BlogPostsGetAll
    {
        Dictionary<string,string> secrets;
        BlogPostContext bpc;

        public BlogPostsGetAll() {
            secrets = GetSecrets.GetSecretsDictionary();
            bpc = GetConnectionString.GetContext(secrets);
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            // ToDo: Not sure this is the best way to do this - probalby ought to use a DI framework.
            context.Logger.LogLine("Get ALL Request\n");
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

            var versions = from blog in bpc.BlogPost
                             group blog by blog.Id into g
                             select new { Id = g.Key, Version = g.Max(x => x.Version) };


            List<BlogPostURL> all = new List<BlogPostURL>();
            foreach(var x in versions) {
                all.Add(new BlogPostURL($"/blog/{x.Id}?version={x.Version}"));
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
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
