using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSBlogCSharp.Database;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MySQL.Data.EntityFrameworkCore;
using System.Text.Json;

//[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSBlogCSharp
{
    class BlogPostsUpdate
    {
        Dictionary<string,string> secrets;
        BlogPostContext bpc;

        public BlogPostsUpdate()
        {
            secrets = GetSecrets.GetSecretsDictionary();
            bpc = GetConnectionString.GetContext(secrets);
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse Update(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Update Request\n");
            APIGatewayProxyResponse response;

            string idStr = request.PathParameters["id"];
            {
                int id = 0;
                if (!Int32.TryParse(idStr, out id))
                {
                    response = new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Body = "Illegal parameter " + id,
                        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                    };
                }
                else
                {
                    var versions = from blog in bpc.BlogPost where (blog.Id == id) && (blog.Status) orderby blog.Version descending select blog;
                    if (versions.Count() == 0)
                    {
                        response = new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Body = "",
                            Headers = new Dictionary<string, string> { }
                        };
                    }
                    else
                    {
                        var latest = versions.First();
                        response = new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonSerializer.Serialize<DBBlogPost>(latest),
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                        };
                    }
                }

            }
            return response;
        }
    }
}
