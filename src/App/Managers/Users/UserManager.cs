using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace App.Managers.Users
{
    public class UserManager : IUserManager
    {
        private readonly ICognitoUserManager _cognitoUserManager;
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly IConfiguration _configuration;


        public UserManager(ICognitoUserManager cognitoUserManager, IAmazonCognitoIdentityProvider cognitoClient, IConfiguration configuration)
        {
            _cognitoUserManager = cognitoUserManager ?? throw new ArgumentNullException(nameof(cognitoUserManager));
            _cognitoClient = cognitoClient ?? throw new ArgumentNullException(nameof(cognitoClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Token> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            //TODO consider getting username from repository to make username entered match case of username in cognito (cognito usernames are case sensitive)

            //Get JWT token from cognito
            return await _cognitoUserManager.LoginAsync(loginRequest, cancellationToken).ConfigureAwait(false);
        }

        public async Task<CreatedUser> CreateAsync(NewUser newUser, CancellationToken cancellationToken)
        {
            if (newUser == null)
                return null;

            //Does user already exist? TODO check repo for existing user by username (case insensitive)

            //Register on cognito
            var signUpResult = await _cognitoUserManager.RegisterAsync(newUser, cancellationToken).ConfigureAwait(false);

            if (signUpResult.FailureReason != null)
                return new CreatedUser { ErrorMessage = signUpResult.FailureReason };


            if (signUpResult == null || string.IsNullOrWhiteSpace(signUpResult.CognitoId))
                return null;

            //Create
            var user = User.Create(newUser, signUpResult.CognitoId);

            //Save TODO SAVE USER TO REPO

            return new CreatedUser { User = user };
        }

        public async Task TestAsync(AuthenticatedUser user, CancellationToken cancellation)
        {
            await Task.CompletedTask;
        }
    }
}
