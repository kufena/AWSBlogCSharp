using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AWSBlogApp2
{
    public class Statics
    {
        public static string access_token { get; set; }
        public static string id_token { get; set; }
        public static string refresh_token { get; set; }
        public static AuthFlowResponse authResponse { get; set; }
        public static ImmutableCredentials creds { get; set; }

        public const string poolID = "eu-west-2_CtEzDKKWR";
        public const string clientID = "398v8proc8mei7m5mdq8vtjo3q";
        public const string identityPoolID = "eu-west-2:5b2ed568-fa52-4376-b719-b74bcc698b92";
        public const string apiURL = @"https://omufctn0kh.execute-api.eu-west-2.amazonaws.com/Prod/";
        
    }
}
