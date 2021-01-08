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
            // We expect a model that fits BlogPostModel - so a version, but no id.
            Console.WriteLine("Create Request\n");
            APIGatewayProxyResponse response;

            Console.WriteLine(request);
            Console.WriteLine("Request body:::" + request.Body);

            BlogPostModel bpm = JsonSerializer.Deserialize<BlogPostModel>( request.Body );
            Console.WriteLine("Deserialized body");
            if (bpm.Version != 0) {
                response = new APIGatewayProxyResponse {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Version {bpm.Version} Not Zero"
                };
            }
            else {
                Console.WriteLine("About to create a new id");
                // we create an id.
                int id = 0; //(new Random()).Next(1000000);            
                var addid = bpc.BlogIds.Add(new DBBlogId("A"));
                int x = bpc.SaveChanges();
                if (x == 0) {
                    Console.WriteLine("No changes made to db - so that's no good!");
                    return new APIGatewayProxyResponse {
                        StatusCode = (int) HttpStatusCode.BadRequest,
                        Body = "Nope, not having the id db thing again"
                    };
                }
                id = addid.CurrentValues.GetValue<int>("Id");

                Console.WriteLine($"New Id created :: {id}");

                // let's save the body text to our S3 bucket in a file of our choosing

                AmazonS3Client s3client = new AmazonS3Client( Amazon.RegionEndpoint.EUWest2 );//S3Region.EUW2);
                var resp = await s3client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest {
                            BucketName = secrets["blogstore"],
                            Key = $"/Blog{id}/Version{bpm.Version}",
                            ContentBody = bpm.Text
                            });

                Console.WriteLine("Written to S3");

                // create a db model
                // save the db model
                
                string inputstring = $"{id}:{DateTime.Now}:{bpm.Version}:{bpm.Title}:{bpm.Text}:";                
                var base64hash = HashBlog.MakeHash(inputstring);
                
                Console.WriteLine("New Hash:::" + base64hash);

                DBBlogPost dbbp = new DBBlogPost(id, bpm.Version, bpm.Title, DateTime.Now, $"/Blog{id}/Version{bpm.Version}", bpm.Status, base64hash);
                bpc.BlogPost.Add(dbbp);
                bpc.SaveChanges();
                Console.WriteLine("Written to DB");

                // create a response containing the new id - perhaps also a URL - maybe just the URL?

                response = new APIGatewayProxyResponse {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = "{ \"URL\": \"/blog/" + $"{id}" + "\" }",
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            return response;
        }
    }
}
