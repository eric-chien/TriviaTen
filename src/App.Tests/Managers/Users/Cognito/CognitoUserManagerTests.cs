using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using App.Managers.Users;
using App.Managers.Users.Cognito;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace App.Tests.Controllers.V1.Users
{
    public class CognitoUserManagerTests
    {
        private readonly Mock<IAmazonCognitoIdentityProvider> _cognitoClientMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly CognitoUserManager _sut;

        protected CognitoUserManagerTests()
        {
            _cognitoClientMock = new Mock<IAmazonCognitoIdentityProvider>(MockBehavior.Strict);
            _configurationMock = new Mock<IConfiguration>(MockBehavior.Strict);
            _sut = new CognitoUserManager(_cognitoClientMock.Object, _configurationMock.Object);
        }

        private static AdminInitiateAuthResponse GenerateAuthResponse()
        {
            var authResult = new AuthenticationResultType
            {
                AccessToken = "test-token",
                ExpiresIn = 123,
                IdToken = "test-token",
                RefreshToken = "test-token"
            };
            var token = new AdminInitiateAuthResponse
            {
                AuthenticationResult = authResult
            };

            return token;
        }

        public class LoginTests : CognitoUserManagerTests
        {
            [Fact]
            public async Task Returns_ValidToken_On_SuccessfulLogin()
            {
                var authResponse = GenerateAuthResponse();
                _cognitoClientMock.Setup(m => m.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(authResponse);
                _configurationMock.Setup(m => m[It.IsAny<string>()]).Returns("test-string");

                var loginRequest = new LoginRequest { Username = "test", Password = "test" };

                var token = await _sut.LoginAsync(loginRequest, CancellationToken.None);
                Assert.IsType<Token>(token);
                Assert.NotNull(token.AccessToken);
                Assert.NotNull(token.RefreshToken);
                Assert.True(token.ExpiresIn > 0);
                Assert.Null(token.ErrorMessage);

                _cognitoClientMock.Verify(m => m.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task Returns_InvalidToken_On_NotAuthorizedException()
            {
                var authResponse = GenerateAuthResponse();
                _cognitoClientMock.Setup(m => m.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotAuthorizedException(""));
                _configurationMock.Setup(m => m[It.IsAny<string>()]).Returns("test-string");

                var loginRequest = new LoginRequest { Username = "test", Password = "test" };

                var token = await _sut.LoginAsync(loginRequest, CancellationToken.None);
                Assert.IsType<Token>(token);
                Assert.Null(token.AccessToken);
                Assert.Null(token.RefreshToken);
                Assert.True(token.ExpiresIn == 0);
                Assert.NotNull(token.ErrorMessage);

                _cognitoClientMock.Verify(m => m.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task Returns_InvalidToken_On_UserNotFoundException()
            {
                var authResponse = GenerateAuthResponse();
                _cognitoClientMock.Setup(m => m.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new UserNotFoundException(""));
                _configurationMock.Setup(m => m[It.IsAny<string>()]).Returns("test-string");

                var loginRequest = new LoginRequest { Username = "test", Password = "test" };

                var token = await _sut.LoginAsync(loginRequest, CancellationToken.None);
                Assert.IsType<Token>(token);
                Assert.Null(token.AccessToken);
                Assert.Null(token.RefreshToken);
                Assert.True(token.ExpiresIn == 0);
                Assert.NotNull(token.ErrorMessage);

                _cognitoClientMock.Verify(m => m.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task Returns_Null_On_AmazonCognitoIdentityProviderException()
            {
                var authResponse = GenerateAuthResponse();
                _cognitoClientMock.Setup(m => m.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new AmazonCognitoIdentityProviderException(""));
                _configurationMock.Setup(m => m[It.IsAny<string>()]).Returns("test-string");

                var loginRequest = new LoginRequest { Username = "test", Password = "test" };

                var token = await _sut.LoginAsync(loginRequest, CancellationToken.None);
                Assert.Null(token);

                _cognitoClientMock.Verify(m => m.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        public class RegisterTests : CognitoUserManagerTests
        {
            [Fact]
            public async Task Returns_ErrorSignUpResult_When_PasswordDoesntMeetRequirements()
            {
                var newUser = new NewUser("test", "password");
                var signUpResult = await _sut.RegisterAsync(newUser, CancellationToken.None);

                Assert.Null(signUpResult.CognitoId);
                Assert.NotNull(signUpResult.ErrorMessage);

                _cognitoClientMock.Verify(m => m.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()), Times.Never);
                _cognitoClientMock.Verify(m => m.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task Returns_ErrorSignUpResult_When_UsernameExistsException()
            {
                _configurationMock.Setup(m => m[It.IsAny<string>()]).Returns("test-string");
                _cognitoClientMock.Setup(m => m.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new UsernameExistsException(""));

                var newUser = new NewUser("test", "Password01");
                var signUpResult = await _sut.RegisterAsync(newUser, CancellationToken.None);

                Assert.Null(signUpResult.CognitoId);
                Assert.NotNull(signUpResult.ErrorMessage);

                _cognitoClientMock.Verify(m => m.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()), Times.Once);
                _cognitoClientMock.Verify(m => m.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task Returns_ErrorSignUpResult_When_AmazonCognitoIdentityProviderException()
            {
                _configurationMock.Setup(m => m[It.IsAny<string>()]).Returns("test-string");
                _cognitoClientMock.Setup(m => m.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new AmazonCognitoIdentityProviderException(""));

                var newUser = new NewUser("test", "Password01");
                var signUpResult = await _sut.RegisterAsync(newUser, CancellationToken.None);

                Assert.Null(signUpResult.CognitoId);
                Assert.NotNull(signUpResult.ErrorMessage);

                _cognitoClientMock.Verify(m => m.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()), Times.Once);
                _cognitoClientMock.Verify(m => m.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task Returns_ErrorSignUpResult_When_FailedToConfirmUser()
            {
                var signUpResponse = new SignUpResponse { UserSub = "test-id" };
                var confirmReponse = new AdminConfirmSignUpResponse { HttpStatusCode = System.Net.HttpStatusCode.InternalServerError };

                _configurationMock.Setup(m => m[It.IsAny<string>()]).Returns("test-string");
                _cognitoClientMock.Setup(m => m.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(signUpResponse);
                _cognitoClientMock.Setup(m => m.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(confirmReponse);

                var newUser = new NewUser("test", "Password01");
                var signUpResult = await _sut.RegisterAsync(newUser, CancellationToken.None);

                Assert.Null(signUpResult.CognitoId);
                Assert.NotNull(signUpResult.ErrorMessage);

                _cognitoClientMock.Verify(m => m.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()), Times.Once);
                _cognitoClientMock.Verify(m => m.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task Returns_Createduser_When_RegistrationSuccessful()
            {
                var signUpResponse = new SignUpResponse { UserSub = "test-id" };
                var confirmReponse = new AdminConfirmSignUpResponse { HttpStatusCode = System.Net.HttpStatusCode.OK };

                _configurationMock.Setup(m => m[It.IsAny<string>()]).Returns("test-string");
                _cognitoClientMock.Setup(m => m.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(signUpResponse);
                _cognitoClientMock.Setup(m => m.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(confirmReponse);

                var newUser = new NewUser("test", "Password01");
                var signUpResult = await _sut.RegisterAsync(newUser, CancellationToken.None);

                Assert.NotNull(signUpResult.CognitoId);
                Assert.Null(signUpResult.ErrorMessage);

                _cognitoClientMock.Verify(m => m.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()), Times.Once);
                _cognitoClientMock.Verify(m => m.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
