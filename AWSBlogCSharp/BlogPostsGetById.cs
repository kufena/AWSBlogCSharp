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

        Dictionary<string,string>secrets;
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
                    bool statusClause = false;
                    bool statusStr = false;
                    if (request.PathParameters.ContainsKey("status")) {
                        if (!Boolean.TryParse(request.PathParameters["status"], out statusStr))
                            return new APIGatewayProxyResponse {
                                StatusCode = (int) HttpStatusCode.BadRequest,
                                Body = "{ \"error\":\"Bad status parameter\"}"
                            };
                        statusClause = true;
                    }
                    IOrderedQueryable<DBBlogPost> versions;
                    if (statusClause)
                        versions = from blog in bpc.BlogPost where (blog.Id == id) && (blog.Status == statusStr) orderby blog.Version descending select blog;
                    else
                        versions = from blog in bpc.BlogPost where (blog.Id == id) orderby blog.Version descending select blog;
                    
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
                        //var resp = await s3client.GetObjectAsync(secrets["blogstore"], "/" + latest.File);
                        var resp = await s3client.GetObjectAsync(new GetObjectRequest { BucketName = "thegatehousewereham.blogposts", Key = latest.File});
                        var outstream = new StreamReader(resp.ResponseStream);
                        var text = outstream.ReadToEnd();
                        var model = new BlogPostModel(latest.Version,latest.Title,latest.Date, text, latest.Status, latest.Hash);
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
