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

            if (Statics.authResponse.AuthenticationResult == null)
            {
                return false;
            }
            Statics.user = user;
            Console.WriteLine(Statics.myidtoken);

            var xxx = await getCredentials();

            return xxx;
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

        public async Task<bool> refreshCredentials()
        {
            if (Statics.credentials != null)
            {
                if (Statics.myidtoken.ValidTo < DateTime.Now)
                {
                    //var p2 = new AmazonCognitoIdentityProviderClient(Statics.credentials);

                    var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Amazon.RegionEndpoint.EUWest2);
                    var userPool = new CognitoUserPool(Statics.poolID, Statics.clientID, provider);
                    var user = new CognitoUser((string)Statics.myidtoken.Payload["email"], Statics.clientID, userPool, provider);
                    //                    var acpc = new AmazonCognitoIdentityProviderClient(Statics.creds.AccessKey, Statics.creds.SecretKey, Statics.creds.Token);
                    user.SessionTokens = new CognitoUserSession(Statics.id_token, Statics.access_token, Statics.refresh_token, 
                        Statics.myidtoken.IssuedAt, DateTime.Now.AddHours(1));
                    //Statics.user.SessionTokens.ExpirationTime = DateTime.Now.AddHours(1);
                    var awr = new InitiateRefreshTokenAuthRequest
                    {
                        AuthFlowType = AuthFlowType.REFRESH_TOKEN
                    };
                    
                    try
                    {
                        //Statics.authResponse = await Statics.user.StartWithRefreshTokenAuthAsync(awr);
                        Statics.authResponse = await user.StartWithRefreshTokenAuthAsync(awr);
                        Statics.user = user;
                        return await getCredentials();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.ToString());
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine(ex.InnerException.Message);
                            Console.WriteLine(ex.InnerException.ToString());
                        }
                    }
                }
                else {
                    var newid = Statics.credentials.RefreshIdentityAsync();
                    return true;
                }
            }
            return false;
        }
    }
}
