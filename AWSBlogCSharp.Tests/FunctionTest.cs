using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using AWSBlogCSharp;

namespace AWSBlogCSharp.Tests
{
    public class FunctionTest
    {
        public FunctionTest()
        {
        }

        [Fact]
        public void TetGetMethod()
        {
            //TestLambdaContext context;
            //APIGatewayProxyRequest request;
            //APIGatewayProxyResponse response;

            //Functions functions = new Functions();

            /*
            request = new APIGatewayProxyRequest();
            context = new TestLambdaContext();
            response = functions.Get(request, context);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Hello AWS Serverless", response.Body);
            */
        }

        [Fact]
        public void TestHashFunctionSame() {
            string p = "here is a long string that should be used for testing a hash function!";
            string q = "here is a long string that should be xsed for testing a hash function?";
            var hash1 = HashBlog.MakeHash(p);
            var hash2 = HashBlog.MakeHash(q);
            Assert.NotEqual(hash1,hash2);
        }
    }
}
