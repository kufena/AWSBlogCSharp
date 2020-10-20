using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSBlogCSharp.Database;
using System;
using System.IO;
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
using System.Threading.Tasks;

//[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSBlogCSharp
{
    class BlogPostsGetById
    {

        Dictionary<string, string> secrets;
        BlogPostContext bpc;

        public BlogPostsGetById()
        {
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
            context.Logger.LogLine("Get Request\n");
            APIGatewayProxyResponse response;

            string idStr = request.PathParameters["id"];

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
                bool statusClause = false;
                bool statusStr = false;
                bool versionClause = false;
                int selectVersion = 0;

                if (!(request.QueryStringParameters is null))
                {
                    if (request.QueryStringParameters.ContainsKey("status"))
                    {
                        if (!Boolean.TryParse(request.QueryStringParameters["status"], out statusStr))
                            return new APIGatewayProxyResponse
                            {
                                StatusCode = (int)HttpStatusCode.BadRequest,
                                Body = "{ \"error\":\"Bad status parameter\"}"
                            };
                        statusClause = true;
                        context.Logger.LogLine("Selection with status = " + statusStr);
                    }

                    if (request.QueryStringParameters.ContainsKey("version"))
                    {
                        if (!Int32.TryParse(request.QueryStringParameters["version"], out selectVersion))
                            return new APIGatewayProxyResponse
                            {
                                StatusCode = (int)HttpStatusCode.BadRequest,
                                Body = "Poor choice of version."
                            };
                        versionClause = true;
                        context.Logger.LogLine("Selecting version " + selectVersion);
                    }
                }

                if (statusClause && versionClause)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Body = "Can't have both status and version clause when choosing."
                    };
                }
                
                DBBlogPost latest = null;

                if (statusClause)
                {
                    var versions = from blog in bpc.BlogPost where (blog.Id == id) && (blog.Status == statusStr) orderby blog.Version descending select blog;
                    if (versions.Count() != 0) latest = versions.First();
                }
                else
                {
                    if (versionClause)
                    {
                        var versions = from blog in bpc.BlogPost where (blog.Id == id) && (blog.Version == selectVersion) select blog;
                        if (versions.Count() != 0) latest = versions.First();
                    }
                    else
                    {
                        var versions = from blog in bpc.BlogPost where (blog.Id == id) orderby blog.Version descending select blog;
                        if (versions.Count() != 0) latest = versions.First();
                    }
                }

                if (latest is null)
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
                    // Fetch file from S3
                    AmazonS3Client s3client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest2);
                    var resp = await s3client.GetObjectAsync(new GetObjectRequest { BucketName = secrets["blogstore"], Key = latest.File });
                    var outstream = new StreamReader(resp.ResponseStream);
                    var text = outstream.ReadToEnd();

                    // Create model
                    var model = new BlogPostModel(latest.Version, latest.Title, latest.Date, text, latest.Status, latest.Hash);
                    
                    // Return response
                    response = new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = JsonSerializer.Serialize<BlogPostModel>(model),
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                    };
                }
            }


            return response;
        }
    }
}
