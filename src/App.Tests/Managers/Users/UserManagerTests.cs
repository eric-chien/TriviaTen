using App.Managers.Users;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace App.Tests.Controllers.V1.Users
{
    public class UserManagerTests
    {
        private readonly Mock<ICognitoUserManager> _cognitoUserManagerMock;
        private readonly UserManager _sut;

        protected UserManagerTests()
        {
            _cognitoUserManagerMock = new Mock<ICognitoUserManager>(MockBehavior.Strict);
            _sut = new UserManager(_cognitoUserManagerMock.Object);
        }

        public class LoginTests : UserManagerTests
        {
            [Fact]
            public async Task Returns_Null_When_LoginRequest_IsNull()
            {
                _cognitoUserManagerMock.Setup(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Token());

                var token = await _sut.LoginAsync(null, CancellationToken.None);
                Assert.Null(token);

                _cognitoUserManagerMock.Verify(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task Returns_Token()
            {
                _cognitoUserManagerMock.Setup(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Token());

                var token = await _sut.LoginAsync(new LoginRequest(), CancellationToken.None);
                Assert.IsType<Token>(token);

                _cognitoUserManagerMock.Verify(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        public class CreateTests : UserManagerTests
        {
            [Fact]
            public async Task Returns_ErrorCreatedUser_When_NewUserIsNull()
            {
                var createdUser = await _sut.CreateAsync(null , CancellationToken.None);

                Assert.Null(createdUser.User);
                Assert.NotNull(createdUser.ErrorMessage);
                
                _cognitoUserManagerMock.Verify(m => m.RegisterAsync(It.IsAny<NewUser>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task Returns_ErrorCreatedUser_When_SignupResult_HasFailureReason()
            {
                var failedSignUpResult = new SignUpResult { ErrorMessage = "Failed to signup user" };
                _cognitoUserManagerMock.Setup(m => m.RegisterAsync(It.IsAny<NewUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(failedSignUpResult);

                var createdUser = await _sut.CreateAsync(new NewUser("test", "test"), CancellationToken.None);

                Assert.Null(createdUser.User);
                Assert.NotNull(createdUser.ErrorMessage);

                _cognitoUserManagerMock.Verify(m => m.RegisterAsync(It.IsAny<NewUser>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task Returns_Null_When_SignUpResult_IsNull_Or_MissingCognitoId()
            {
                _cognitoUserManagerMock.Setup(m => m.RegisterAsync(It.IsAny<NewUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SignUpResult());

                var createdUser = await _sut.CreateAsync(new NewUser("test", "test"), CancellationToken.None);

                Assert.Null(createdUser);

                _cognitoUserManagerMock.Verify(m => m.RegisterAsync(It.IsAny<NewUser>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task Returns_CreatedUser_When_SignUpSuccessful()
            {
                var signUpResult = new SignUpResult { CognitoId = "test-id" };
                _cognitoUserManagerMock.Setup(m => m.RegisterAsync(It.IsAny<NewUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(signUpResult);

                var createdUser = await _sut.CreateAsync(new NewUser("test", "test"), CancellationToken.None);

                Assert.NotNull(createdUser.User);
                Assert.Null(createdUser.ErrorMessage);

                _cognitoUserManagerMock.Verify(m => m.RegisterAsync(It.IsAny<NewUser>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
