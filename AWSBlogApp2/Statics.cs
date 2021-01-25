using Amazon.Extensions.CognitoAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSBlogApp2
{
    public class Statics
    {
        public static string access_token { get; set; }
        public static string id_token { get; set; }
        public static string refresh_token { get; set; }
        public static AuthFlowResponse authResponse { get; set; }

        public const string poolID = "eu-west-2_epJB66dgN";
        public const string clientID = "413g34k4oo41cdte2ec8phe5k2";
        public const string identityPoolID = "eu-west-2:1a7dad11-82f8-48f3-84c6-a37c9064ac2a";
        public const string apiURL = @"https://rwvva2pgl3.execute-api.eu-west-2.amazonaws.com/Prod/";
    }
}
