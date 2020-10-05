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

[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSBlogCSharp
{
    class BlogPostsGetAll
    {
        BlogPostContext bpc;

        public BlogPostsGetAll() {
            var connectionString = GetConnectionString.GetSecret();
            Console.WriteLine(connectionString);
            var options = new DbContextOptionsBuilder<BlogPostContext>();
            options.UseMySQL(connectionString);
            bpc = new BlogPostContext(options.Options);
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            // ToDo: Not sure this is the best way to do this - probalby ought to use a DI framework.
            context.Logger.LogLine("Get ALL Request\n");
            int c = -19929;

            //using (bpc)
            //{
                context.Logger.LogLine("Inside the using bit.");
                var x = from blog in bpc.BlogPost select blog;
                context.Logger.LogLine("Now we've done the select");
                c = x.Count();
                context.Logger.LogLine("Now we have c - oh, need to declare that outside don't I");
            //}

            context.Logger.LogLine("{ \"fred\": \"All the posts from the fair. " + c + "\" }");
            
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"fred\": \"All the posts from the fair. " + c + "\" }",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };

            return response;
        }
    }
}
