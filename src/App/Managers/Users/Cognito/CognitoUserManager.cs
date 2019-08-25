using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace App.Managers.Users.Cognito
{
    public class CognitoUserManager : ICognitoUserManager
    {
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly IConfiguration _configuration;
        
        public CognitoUserManager(IAmazonCognitoIdentityProvider cognitoClient, IConfiguration configuration)
        {
            _cognitoClient = cognitoClient ?? throw new ArgumentNullException(nameof(cognitoClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Token> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            //Create authentication request
            var request = new AdminInitiateAuthRequest
            {
                UserPoolId = _configuration[ConfigurationKeys.CognitoUserPoolId],
                ClientId = _configuration[ConfigurationKeys.CognitoClientId],
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
            };
            request.AuthParameters.Add("USERNAME", loginRequest.Username);
            request.AuthParameters.Add("PASSWORD", loginRequest.Password);

            try //Try to authenticate... Cognito client throws on any errors instead of returning an invalid response...
            {
                //Request authentication
                var response = await _cognitoClient.AdminInitiateAuthAsync(request, cancellationToken).ConfigureAwait(false);

                return new Token
                {
                    AccessToken = response.AuthenticationResult.AccessToken,
                    ExpiresIn = response.AuthenticationResult.ExpiresIn,
                    RefreshToken = response.AuthenticationResult.RefreshToken
                };
            }
            catch (NotAuthorizedException)
            {
                return new Token
                {
                    ErrorMessage = "Invalid password"
                };
            }
            catch (UserNotFoundException)
            {
                return new Token
                {
                    ErrorMessage = "User not found"
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public async Task<SignUpResult> RegisterAsync(NewUser newUser, CancellationToken cancellationToken)
        {
            //Ensure password meets criteria
            if (!ValidPassword(newUser.Password))
                return new SignUpResult { ErrorMessage = "Password does not meet password criteria" };

            //Create the user account in cognito
            var signUpResult = await SignupUserAsync(newUser, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(signUpResult?.CognitoId))
                return new SignUpResult { ErrorMessage = signUpResult?.ErrorMessage };

            //Confirm the user in cognito to allow login (no email/sms confirmation required)
            var successfulConfirm = await ConfirmUserAsync(newUser.Username, cancellationToken).ConfigureAwait(false);

            if (!successfulConfirm)
                return new SignUpResult { ErrorMessage = "Failed to confirm user" };

            return signUpResult;
        }

        #region helpers

        private async Task<SignUpResult> SignupUserAsync(NewUser newUser, CancellationToken cancellationToken)
        {
            //Create signup request
            var signupRequest = new SignUpRequest
            {
                Username = newUser.Username,
                Password = newUser.Password,
                ClientId = _configuration[ConfigurationKeys.CognitoClientId]
            };

            try //Try to create user... Cognito client throws on any errors instead of returning an invalid response...
            {
                var response = await _cognitoClient.SignUpAsync(signupRequest, cancellationToken).ConfigureAwait(false);

                return new SignUpResult
                {
                    CognitoId = response.UserSub
                };
            }
            catch (UsernameExistsException)
            {
                return new SignUpResult
                {
                    ErrorMessage = "Username already exists"
                };
            }
            catch (Exception)
            {
                return new SignUpResult
                {
                    ErrorMessage = "Failed to sign up user"
                };
            }
        }

        private async Task<bool> ConfirmUserAsync(string userName, CancellationToken cancellationToken)
        {
            var confirmRequest = new AdminConfirmSignUpRequest
            {
                Username = userName,
                UserPoolId = _configuration[ConfigurationKeys.CognitoUserPoolId]
            };
            var response = await _cognitoClient.AdminConfirmSignUpAsync(confirmRequest);

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return false;

            return true;
        }

        private bool ValidPassword(string password)
        {
            //Passwords must be at least 6 letters, contain an uppercase letter, lowercase letter, and a number.
            if (password.Length < 6)
                return false;

            if (!password.Any(char.IsUpper))
                return false;

            if (!password.Any(char.IsLower))
                return false;

            if (!password.Any(char.IsDigit))
                return false;

            return true;
        }
        
        #endregion helpers
    }
}
