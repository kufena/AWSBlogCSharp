using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace AWSBlogApp2
{
    public class Authentication
    {
        SessionState Statics;

        public Authentication(SessionState state)
        {
            this.Statics = state;
        }

        //
        // Given user details, register a user to the cognito user pool.
        //
        public async Task<bool> registerUser(string nickname, string password, string email)
        {
            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Amazon.RegionEndpoint.EUWest2);
            var userPool = new CognitoUserPool(Statics.poolID, Statics.clientID, provider);

            var useratts = new Dictionary<string, string>();
            useratts.Add("nickname", nickname);

            Console.WriteLine($"Adding user {email} nickname {nickname}");
            try
            {
                await userPool.SignUpAsync(email, password, useratts, new Dictionary<string, string>());
                Console.WriteLine("Done sign up async.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong.");
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        //
        // Given an email address and confirmation code, confirm the user.
        public async Task<bool> confirmUser(string email, string code)
        {
            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Amazon.RegionEndpoint.EUWest2);
            var userPool = new CognitoUserPool(Statics.poolID, Statics.clientID, provider);

            var user = new CognitoUser(email, Statics.clientID, userPool, provider);

            Console.WriteLine($"Adding user {email} confirmation");
            try
            {
                await user.ConfirmSignUpAsync(code, false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            //await userPool.SignUpAsync(email, password, attributes, new Dictionary<string,string>());
            Console.WriteLine("Done confirm async.");

            return true;
        }

        public async Task<bool> loginUser(string email, string password)
        {
            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Amazon.RegionEndpoint.EUWest2);
            var userPool = new CognitoUserPool(Statics.poolID, Statics.clientID, provider);
            var user = new CognitoUser(email, Statics.clientID, userPool, provider);

            try
            {
                Statics.authResponse = await user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest()
                {
                    Password = password
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);
                return false;
            }

            Amazon.CognitoIdentityProvider.Model.AuthenticationResultType
                x = Statics.authResponse.AuthenticationResult;
            
            if (Statics.authResponse.AuthenticationResult == null)
            {
                return false;
            }

            Statics.access_token = Statics.authResponse.AuthenticationResult.AccessToken;
            Statics.id_token = Statics.authResponse.AuthenticationResult.IdToken;
            Statics.refresh_token = Statics.authResponse.AuthenticationResult.RefreshToken;

            Statics.myidtoken = new JwtSecurityToken(Statics.id_token);
            var myaccesstoken = new JwtSecurityToken(Statics.access_token);
            Console.WriteLine(Statics.myidtoken);
            Console.WriteLine(myaccesstoken);

            return await getCredentials();

        }

        public async Task<bool> getCredentials()
        {
            var credentials = new CognitoAWSCredentials(Statics.identityPoolID, 
                                                        RegionEndpoint.EUWest2);

            var providername = 
                $"cognito-idp.{RegionEndpoint.EUWest2.SystemName}.amazonaws.com/{Statics.poolID}";
            Console.WriteLine(providername);
            credentials.AddLogin(providername, Statics.id_token);
            var creds = credentials.GetCredentials();
            Statics.creds = creds;
            Statics.credentials = credentials;

            Console.WriteLine("log in id::" + credentials);
            Console.WriteLine(creds);

            return true;
        }

        public async void refreshCredentials()
        {
            if (Statics.credentials != null)
            {
                var newid = Statics.credentials.RefreshIdentityAsync();
            }
        }
    }
}
