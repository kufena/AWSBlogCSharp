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
            bpc = GetConnectionString.GetContext(secrets);  
        }
        
        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task<APIGatewayProxyResponse> Create(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (request is null) {
                Console.WriteLine("The Effffing Request is Null - Can't do much here then.");
                return new APIGatewayProxyResponse {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "No effing request object - it's a null!"
                };
            }
            else
            {
                Console.WriteLine("The request object is not null at least.");
            }
            // We expect a model that fits BlogPostModel - so a version, but no id.
            Console.WriteLine("Create Request\n");
            APIGatewayProxyResponse response;
            
            if (request is null) {
                Console.WriteLine("The Effffing Request is Null - Can't do much here then.");
                return new APIGatewayProxyResponse {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "No effing request object - it's a null!"
                };
            }
            else
            {
                Console.WriteLine("The request object is not null at least.");
            }

            if (request.Headers is null) {
                Console.WriteLine("The Effffing Request Headers is Null - Can't do much here then.");
                return new APIGatewayProxyResponse {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "No effing request object - it's a null!"
                };
            }
            else
            {
                Console.WriteLine("The request headers object is not null at least.");
            }

            foreach(var key in request.Headers.Keys) {
                Console.WriteLine($"Header:: {key} -> {request.Headers[key]}");
            }
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
                int id = 0;            
                var addid = bpc.BlogId.Add(new DBBlogId());
                bpc.SaveChanges();
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
                DBBlogPost dbbp = new DBBlogPost(id, bpm.Version, bpm.Title, DateTime.Now, $"/Blog{id}/Version{bpm.Version}", bpm.Status );
                bpc.BlogPost.Add(dbbp);
                bpc.SaveChanges();
                Console.WriteLine("Written to DB");

                // create a response containing the new id - perhaps also a URL - maybe just the URL?

                response = new APIGatewayProxyResponse {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = "{ \"URL\" = \"/blog/" + $"{id}" + "\" }",
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            return response;
        }
    }
}
