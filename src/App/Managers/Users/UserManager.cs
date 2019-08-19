using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace App.Managers.Users
{
    public class UserManager : IUserManager
    {
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly IConfiguration _configuration;


        public UserManager(IAmazonCognitoIdentityProvider cognitoClient, IConfiguration configuration)
        {
            _cognitoClient = cognitoClient ?? throw new ArgumentNullException(nameof(cognitoClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Token> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            //TODO consider getting username from repository to make username entered match case of username in cognito (cognito usernames are case sensitive)

            return await PerformLoginAsync(loginRequest, cancellationToken).ConfigureAwait(false);
        }

        public async Task<User> RegisterAsync(NewUser newUser, CancellationToken cancellationToken)
        {
            if (newUser == null)
                return null;

            //Does user already exist? TODO check repo for existing user by username (case insensitive)

            if (!ValidPassword(newUser.Password))
                throw new ArgumentException($"Password does not meet password criteria");

            //Create the user account in cognito
            var cognitoId = await SignupUserAsync(newUser, cancellationToken).ConfigureAwait(false);
            
            if (string.IsNullOrEmpty(cognitoId))
                return null;

            //Confirm the user in cognito to allow login (no email/sms confirmation required)
            var successfulConfirm = await ConfirmUserAsync(newUser.Username, cancellationToken).ConfigureAwait(false);

            if (!successfulConfirm)
                return null;

            //Create the user for TriviaTen domain
            var createdUser = await CreateUserAsync(newUser, cognitoId, cancellationToken).ConfigureAwait(false);

            return createdUser;
        }

        #region helpers

        private async Task<Token> PerformLoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            var request = new AdminInitiateAuthRequest
            {
                UserPoolId = _configuration[ConfigurationKeys.CognitoUserPoolId],
                ClientId = _configuration[ConfigurationKeys.CognitoClientId],
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
            };
            request.AuthParameters.Add("USERNAME", loginRequest.Username);
            request.AuthParameters.Add("PASSWORD", loginRequest.Password);

            try
            {
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
                    FailureReason = "Invalid password"
                };
            }
            catch (UserNotFoundException)
            {
                return new Token
                {
                    FailureReason = "User not found"
                };
            }
            catch (AmazonCognitoIdentityProviderException)
            {
                return null;
            }
        }

        private async Task<User> CreateUserAsync(NewUser newUser, string cognitoId, CancellationToken cancellationToken)
        {
            //Create
            var user = User.Create(newUser, cognitoId);

            //Save TODO SAVE USER TO REPO

            return await Task.FromResult(user).ConfigureAwait(false);
        }

        private async Task<string> SignupUserAsync(NewUser newUser, CancellationToken cancellationToken)
        {
            var signupRequest = new SignUpRequest
            {
                Username = newUser.Username,
                Password = newUser.Password,
                ClientId = _configuration[ConfigurationKeys.CognitoClientId]
            };
            var response = await _cognitoClient.SignUpAsync(signupRequest, cancellationToken).ConfigureAwait(false);

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return await Task.FromResult(string.Empty).ConfigureAwait(false);

            return await Task.FromResult(response.UserSub).ConfigureAwait(false);
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
                return await Task.FromResult(false).ConfigureAwait(false);

            return await Task.FromResult(true).ConfigureAwait(false);
        }

        private bool ValidPassword(string password)
        {
            //Passwords must be greater than 6 letters and contain an uppercase letter, lowercase letter, and a number.
            if (password.Length < 7)
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
