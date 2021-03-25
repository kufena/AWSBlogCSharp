using Amazon.CognitoIdentity;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace AWSBlogApp2
{
    public class SessionState
    {
        public SessionState() { }
        public string access_token { get => (user.SessionTokens.AccessToken); }
        public string id_token { get => (user == null || user.SessionTokens == null ? null : user.SessionTokens.IdToken); }
        public string refresh_token { get => (user.SessionTokens.RefreshToken); }
        public AuthFlowResponse authResponse { get; set; }
        public ImmutableCredentials creds { get; set; }
        public JwtSecurityToken myidtoken { get => (new JwtSecurityToken(user.SessionTokens.IdToken)); }
        public CognitoAWSCredentials credentials { get; set; }

        public CognitoUser user { get; set; }

        public string poolID = "eu-west-2_Ztx8PsrKU";
        public string clientID = "7fh8uega51v8dhaqddhv3cdse8";
        public string identityPoolID = "eu-west-2:8f7f325c-d76e-4b12-8b88-b9ce29cfbfe5";
        public string apiURL = @"https://4x02wmhwbc.execute-api.eu-west-2.amazonaws.com/Prod/";
               
    }
}
