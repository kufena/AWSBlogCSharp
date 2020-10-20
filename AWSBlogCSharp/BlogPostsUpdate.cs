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
using System.Threading.Tasks;
using AWSBlogCSharp.Model;
using Amazon.S3;

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
        public async Task<APIGatewayProxyResponse> Update(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Update Request\n");
            APIGatewayProxyResponse response;
            Console.WriteLine(request);
            Console.WriteLine("Request body:::" + request.Body);

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
            
            BlogPostModel bpm = JsonSerializer.Deserialize<BlogPostModel>( request.Body );
            Console.WriteLine("Deserialized body");

            Console.WriteLine($"Updating blog with Id :: {id}");

            // create a db model
            string inputstring = $"{id}:{bpm.Version+1}:{DateTime.Now}:{bpm.Title}:{bpm.Text}:{bpm.Hash}";                
            var base64hash = HashBlog.MakeHash(inputstring);
            Console.WriteLine("New has will be:::" + base64hash);

            
            // do some checking - last hash, new key.
            var latesthash = (from blog in bpc.BlogPost where (blog.Id == id) orderby blog.Date descending select blog).First<DBBlogPost>().Hash;
            bool condition = string.Compare(latesthash, bpm.Hash) == 0; // need identical hashes.
            Console.WriteLine("Comparing " + latesthash + " and " + bpm.Hash);
            Console.WriteLine($"Condition is now {condition}");
            int newVersion = bpm.Version + 1;
            var newkey = from blog in bpc.BlogPost where (blog.Id == id) && (blog.Version == newVersion) select blog;
            condition = condition && (newkey.ToList<DBBlogPost>().Count == 0);
            Console.WriteLine($"Condition after key test is now {condition}");
            
            if (!condition) {
                return new APIGatewayProxyResponse {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "Either duplicate key or the hash is NOT the latest hash!"
                };
            }

            // save the db model
            DBBlogPost dbbp = new DBBlogPost(id, newVersion, bpm.Title, DateTime.Now, $"/Blog{id}/Version{newVersion}", bpm.Status, base64hash );
            try {
                bpc.BlogPost.Add(dbbp);
                bpc.SaveChanges();
                Console.WriteLine("Written to DB");
            } catch (Exception ex) {
                //bpc = GetConnectionString.GetContext(secrets);
                return new APIGatewayProxyResponse {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    Body = "{ \"Exception\": \""+ex.GetBaseException().ToString()+"\" " +
                               ((!(ex.InnerException is null)) ? ("\"Inner\":\"" + ex.InnerException.ToString() + "\""): "") + "}"
                };
            }
            // let's save the body text to our S3 bucket in a file of our choosing

            AmazonS3Client s3client = new AmazonS3Client( Amazon.RegionEndpoint.EUWest2 );//S3Region.EUW2);
            var resp = await s3client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest {
                        BucketName = secrets["blogstore"],
                        Key = $"/Blog{id}/Version{newVersion}",
                        ContentBody = bpm.Text
                        });

            Console.WriteLine("Written to S3");

            // create a response containing the new id - perhaps also a URL - maybe just the URL?

            response = new APIGatewayProxyResponse {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = "{ \"URL\": \"/blog/" + $"{id}?version={newVersion}" + "\" }",
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        
            return response;
        }
    }
}
