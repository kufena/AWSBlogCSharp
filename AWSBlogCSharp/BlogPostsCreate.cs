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
using AWSBlogModel;
using Amazon.S3;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

//[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSBlogCSharp
{
    class BlogPostsCreate
    {
        BlogPostContext bpc;
        Dictionary<string,string> secrets;

        public BlogPostsCreate()
        {
            secrets = GetSecrets.GetSecretsDictionary();
            string connstr = GetSecrets.GetSecretConnectionString();
            bpc = GetConnectionString.GetContext(connstr);  
        }
        
        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task<APIGatewayProxyResponse> Create(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string user = request.PathParameters["user"];
            context.Logger.LogLine($"USER PARAMETER IS =--= {user}");

            // We expect a model that fits BlogPostModel - so a version, but no id.
            Console.WriteLine("Create Request\n");
            APIGatewayProxyResponse response;

            Console.WriteLine(request);
            Console.WriteLine("Request body:::" + request.Body);

            BlogPostModel bpm = JsonSerializer.Deserialize<BlogPostModel>( request.Body );
            Console.WriteLine("Deserialized body");
            Console.WriteLine($"{bpm.Title} {bpm.Date} {bpm.Text} {bpm.Status}");

            if (bpm.Version != 0) {
                response = new APIGatewayProxyResponse {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Version {bpm.Version} Not Zero"
                };
            }
            else
            {
                Console.WriteLine("About to create a new id");
                // we create an id.
                int id = 0; //(new Random()).Next(1000000);            
                var addid = bpc.BlogIds.Add(new DBBlogId("A"));
                int x = 0;

                try
                {
                    x = bpc.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.ToString());
                    if (e.InnerException != null)
                    {
                        Console.WriteLine("===================");
                        Console.WriteLine(e.InnerException.Message);
                        Console.WriteLine(e.InnerException.ToString());
                    }
                }

                if (x == 0)
                {
                    Console.WriteLine("No changes made to db - so that's no good!");
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Body = "Nope, not having the id db thing again"
                    };
                }
                id = addid.CurrentValues.GetValue<int>("Id");

                Console.WriteLine($"New Id created :: {id}");

                string fileKey = Utilities.MakeBlogFileName(user, id, bpm.Version); 
                
                // let's save the body text to our S3 bucket in a file of our choosing

                AmazonS3Client s3client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest2);//S3Region.EUW2);
                var resp = await s3client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = secrets["blogstore"],
                    Key = fileKey,
                    ContentBody = bpm.Text
                });

                Console.WriteLine("Written to S3");

                // create a db model
                // save the db model

                string base64hash = Utilities.CreateBlogPostHash(user, bpm, id);

                try { 
                    DBBlogPost dbbp = new DBBlogPost(id, bpm.Version, bpm.Title, DateTime.Now, fileKey, bpm.Status, base64hash, user);
                    bpc.BlogPost.Add(dbbp);
                    bpc.SaveChanges();
                    Console.WriteLine("Written to DB");
                    
                }
                catch (Exception e)
                {
                    context.Logger.LogLine(e.Message);
                    context.Logger.LogLine(e.ToString());
                    if (e.InnerException != null)
                    {
                        context.Logger.LogLine("===================");
                        context.Logger.LogLine(e.InnerException.Message);
                        context.Logger.LogLine(e.InnerException.ToString());
                    }
                    return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.BadRequest };
                }
                // create a response containing the new id - perhaps also a URL - maybe just the URL?

                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "{ \"URL\": \"/blog/" + $"{id}" + "\" }",
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" }
                                                           , { "Access-Control-Allow-Origin" , "*" }}
                };
            }
            return response;
        }
    }
}
