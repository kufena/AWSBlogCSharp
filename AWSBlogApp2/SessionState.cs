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

        public string poolID = "eu-west-2_Xf5hiOI0w";
        public string clientID = "7dbtqls1deh3cq5ep7g5c1qqe0";
        public string identityPoolID = "eu-west-2:1bd6fd52-1888-40ea-8945-cfbf744bad06";
        public string apiURL = @"https://7le2dptssk.execute-api.eu-west-2.amazonaws.com/Prod/";
    }
}
