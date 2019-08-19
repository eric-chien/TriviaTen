using System;
using System.Linq;
using System.Security.Claims;

namespace App
{
    public class AuthenticatedUser
    {
        private AuthenticatedUser()
        {
        }

        public Guid Id { get; private set; }
        public string CognitoId { get; private set; }

        public static implicit operator AuthenticatedUser(ClaimsPrincipal value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!value.Identity.IsAuthenticated)
                throw new Exception("User not authenticated");

            var cognitoId = value.Claims.FirstOrDefault(c => string.Equals(c.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))?.Value;

            return new AuthenticatedUser
            {
                CognitoId = cognitoId
            };
        }
    }
}
