using System;
using System.Threading;
using System.Threading.Tasks;
using App.Managers.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers.V1.Users
{
    /// <summary>
    ///     Users controller
    /// </summary>
    [ApiVersion(Version)]
    [Route("v{version:apiVersion}/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private const string Version = "1";

        private readonly IUserManager _userManager;

        public UsersController(IUserManager userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <summary>
        ///     Initiates a login request to get a valid JWT token from cognito
        /// </summary>
        /// <response code="201">If the login was successful and a token was generated</response>
        /// <response code="400">If the login was unsuccessful</response>
        /// <returns>
        ///     Jwt bearer token
        /// </returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public async Task<ActionResult<Token>> Login(LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            var token = await _userManager.LoginAsync(loginRequest, cancellationToken);

            if (token == null || token.FailureReason != null)
                return BadRequest(token);

            return token;
        }

        /// <summary>
        ///     Registers a new user in cognito as well as TriviaTen domain
        /// </summary>
        /// <response code="201">If the registration was successful</response>
        /// <response code="400">If the user parameters were invalid</response>
        /// <response code="500">If the registration was unsuccessful</response>
        /// <returns>
        ///     Created User
        /// </returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(Models.User), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Models.User>> Register(Models.NewUser newModel, CancellationToken cancellationToken)
        {
            var newUser = Models.NewUser.Convert(newModel);

            var user = await _userManager.RegisterAsync(newUser, cancellationToken);

            if (user == null)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            
            var model = Models.User.Convert(user);

            return model;
        }

        [HttpGet("locked")]
        public async Task<ActionResult<string>> Locked (CancellationToken cancellationToken)
        {
            return await Task.FromResult("test");
        }
    }
}
