using App.Controllers.V1.Users;
using App.Managers.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace App.Tests.Controllers.V1.Users
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly UsersController _sut;

        protected UsersControllerTests()
        {
            _userManagerMock = new Mock<IUserManager>(MockBehavior.Strict);
            _sut = new UsersController(_userManagerMock.Object).SetupTestControllerContext();
        }

        public class LoginTests : UsersControllerTests
        {
            [Fact]
            public async Task Login_Returns_BadRequest_When_TokenIsNull()
            {
                _userManagerMock.Setup(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(default(Token));

                var response = await _sut.LoginAsync(new LoginRequest(), CancellationToken.None);
                Assert.IsType<BadRequestObjectResult>(response.Result);

                _userManagerMock.Verify(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task Login_Returns_BadRequest_When_TokenHasFailureReason()
            {
                var invalidToken = new Token
                {
                    FailureReason = "Failed to generate token"
                };

                _userManagerMock.Setup(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(invalidToken);

                var response = await _sut.LoginAsync(new LoginRequest(), CancellationToken.None);
                Assert.IsType<BadRequestObjectResult>(response.Result);

                _userManagerMock.Verify(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task Login_Returns_Token_When_TokenisGenerated()
            {
                _userManagerMock.Setup(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Token());

                var response = await _sut.LoginAsync(new LoginRequest(), CancellationToken.None);
                var token = response.Value;
                Assert.NotNull(token);

                _userManagerMock.Verify(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        public class RegisterTests : UsersControllerTests
        {
            [Fact]
            public async Task Login_Returns_InternalServerError_When_UserIsNull()
            {
                _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<NewUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(default(CreatedUser));

                var newUser = new App.Controllers.V1.Users.Models.NewUser
                {
                    Username = "test",
                    Password = "test"
                };
                //TODO Figure out why cannot access StatusCode off of StatusCodeResult object.
            }
        }
    }
}
