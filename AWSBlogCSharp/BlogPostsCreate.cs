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
using AWSBlogCSharp.Model;
using Amazon.S3;

//[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSBlogCSharp
{
    class BlogPostsCreate
    {
        BlogPostContext bpc;
        Dictionary<string,string> secrets;

        public BlogPostsCreate()
        {
            secrets = GetSecrets.GetSecrets();
            bpc = GetConnectionString.GetConnectionString(secrets);
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse Put(APIGatewayProxyRequest request, ILambdaContext context)
        {
            // We expect a model that fits BlogPostModel - so a version, but no id.
            context.Logger.LogLine("Create Request\n");
            APIGatewayProxyResponse response;

            BlogPostModel bpm = JsonSerializer.Deserialize<BlogPostModel>( request.Body );
            if (bpm.Version != 0) {
                response = new APIGatewayProxyResponse {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Version {bpm.Version} Not Zero"
                };
            }
            else {
                // we create an id.
                int id = 0;            
                var addid = bpc.BlogIds.Add(new DBBlogId());
                bpc.SaveChanges();
                id = addid.CurrentValues.GetValue<int>("Id");

                // let's save the body text to our S3 bucket in a file of our choosing

                AmazonS3Client s3client = new AmazonS3Client( Amazon.RegionEndpoint.EUWest2 );//S3Region.EUW2);
                var resp = s3client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest {
                            BucketName = secrets["blogstore"],
                            Key = $"/Blog{id}/Version{bpm.Version}",
                            ContentBody = bpm.Text
                            });

                // create a db model
                // save the db model
                DBBlogPost dbbp = new DBBlogPost(id, bpm.Version, bpm.Title, DateTime.Now, $"/Blog{id}/Version{bpm.Version}", bpm.Status );
                bpc.BlogPost.Add(dbbp);
                bpc.SaveChanges();
                
                // create a response containing the new id - perhaps also a URL - maybe just the URL?

                response = new APIGatewayProxyResponse {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = "{ \"URL\" = \"/blog/" + $"{id}" + "\" }",
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                }
            }
            return response;
        }
    }
}
