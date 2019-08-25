using System;
using System.Threading;
using System.Threading.Tasks;

namespace App.Managers.Users
{
    public class UserManager : IUserManager
    {
        private readonly ICognitoUserManager _cognitoUserManager;


        public UserManager(ICognitoUserManager cognitoUserManager)
        {
            _cognitoUserManager = cognitoUserManager ?? throw new ArgumentNullException(nameof(cognitoUserManager));
        }

        public async Task<Token> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            if (loginRequest == null)
                return null;

            //TODO consider getting username from repository to make username entered match case of username in cognito (cognito usernames are case sensitive)

            //Get JWT token from cognito
            return await _cognitoUserManager.LoginAsync(loginRequest, cancellationToken).ConfigureAwait(false);
        }

        public async Task<CreatedUser> CreateAsync(NewUser newUser, CancellationToken cancellationToken)
        {
            if (newUser == null)
                return new CreatedUser { ErrorMessage = "Invalid request parameters" };

            //Does user already exist? TODO check repo for existing user by username (case insensitive)

            //Register on cognito
            var signUpResult = await _cognitoUserManager.RegisterAsync(newUser, cancellationToken).ConfigureAwait(false);

            if (signUpResult?.ErrorMessage != null)
                return new CreatedUser { ErrorMessage = signUpResult.ErrorMessage };
            
            if (string.IsNullOrWhiteSpace(signUpResult?.CognitoId))
                return null;

            //Create
            var user = User.Create(newUser, signUpResult.CognitoId);

            //Save TODO SAVE USER TO REPO

            return new CreatedUser { User = user };
        }
    }
}
