using System.Threading;
using System.Threading.Tasks;

namespace App.Managers.Users
{
    public interface ICognitoUserManager
    {
        Task<Token> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken);
        Task<SignUpResult> RegisterAsync(NewUser newUser, CancellationToken cancellationToken);
    }
}
