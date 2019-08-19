using App.Managers.Users;
using System;

namespace App.Controllers.V1.Users.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public UserSettings Settings { get; set; }
        public SupporterType.TypeCode ActiveSupporterType { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public static User Convert(Managers.Users.User user)
        {
            if (user == null)
                return null;

            return new User
            {
                Id = user.Id,
                Username = user.Username,
                Settings = user.Settings,
                ActiveSupporterType = user.ActiveSupporterType,
                CreatedOnUtc = user.CreatedOnUtc
            };
        }
    }
}
