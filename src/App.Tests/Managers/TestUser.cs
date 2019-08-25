using System.Linq;
using System.Security.Claims;

namespace App.Tests.Managers
{
    public static class TestUser
    {
        public static ClaimsPrincipal ClaimsPrincipal { get; } = Create("Test-Cognito-Id");
        public static AuthenticatedUser Instance { get; } = ClaimsPrincipal;

        public static ClaimsPrincipal Create(string cognitoId)
        {
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                cognitoId != null ? new Claim(ClaimTypes.NameIdentifier, cognitoId) : null,
            }.Where(c => c != null), "test");

            return new ClaimsPrincipal(claimsIdentity);
        }

    }
}
