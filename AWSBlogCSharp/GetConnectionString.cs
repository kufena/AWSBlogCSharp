using System;
using System.Collections.Generic;
using System.IO;
using System.Data.Common;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using AWSBlogCSharp.Database;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

/*
*	Use this code snippet in your app.
*	If you need more information about configurations or implementing the sample code, visit the AWS docs:
*	https://aws.amazon.com/developers/getting-started/net/
*	
*	Make sure to include the following packages in your code.
*	
*	using System;
*	using System.IO;
*
*	using Amazon;
*	using Amazon.SecretsManager;
*	using Amazon.SecretsManager.Model;
*
*/
namespace AWSBlogCSharp
{
    class GetConnectionString
    {

        /*
         * AWSSDK.SecretsManager version="3.3.0" targetFramework="net45"
         */
        public static BlogPostContext GetContext(string secrets)
        {
            var options = new DbContextOptionsBuilder<BlogPostContext>();
            var secretJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(secrets);
            string host = secretJson["host"];
            string uid = secretJson["username"];
            string pwd = secretJson["password"];
            string port = secretJson["port"];
            string connstring = $"Server={host};Port={port};Database=GateHouseWerehamBlog;Uid={uid};Pwd={pwd};";
            options.UseMySQL(connstring);
            return new BlogPostContext(options.Options);
        }
    }
}
