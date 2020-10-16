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
using Amazon.S3;
using Amazon.S3.Model;
using AWSBlogCSharp.Model;

//[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSBlogCSharp
{
    class BlogPostsGetById
    {

        Dictionary<string,string>secrets;
        BlogPostContext bpc;

        public BlogPostsGetById()
        {
            secrets = GetSecrets.GetSecretsDictionary();
            bpc = GetConnectionString.GetContext(secrets);        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get Request\n");
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
                        AmazonS3Client s3client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest2);
                        var resp = s3client.GetObjectAsync(secrets["blogstore"],
                                                           latest.File);

                        var model = new BlogPostModel(latest.Version,latest.Title,latest.Date, "", latest.Status);
                        response = new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonSerializer.Serialize<BlogPostModel>(model),
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                        };
                    }
                }

            }
            return response;
        }
    }
}
