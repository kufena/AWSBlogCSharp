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

        public const string poolID = "eu-west-2_Evqd08jV3";
        public const string clientID = "fe0d3ge8amg5f3stun2apdin5";
        public const string identityPoolID = "eu-west-2:e7ae8ff9-4eb2-49e3-92f0-cfafcdc89355";
        public const string apiURL = @"https://nocovzsnq6.execute-api.eu-west-2.amazonaws.com/Prod/";
    }
}
