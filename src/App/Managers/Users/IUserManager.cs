﻿using System.Threading;
using System.Threading.Tasks;

namespace App.Managers.Users
{
    public interface IUserManager
    {
        Task<Token> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken);
        Task<User> RegisterAsync(NewUser newUser, CancellationToken cancellationToken);
    }
}
