using App.Managers.Users;

namespace App.Controllers.V1.Users.Models
{
    public class NewUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserSettings Settings { get; set; }

        public static Managers.Users.NewUser Convert(NewUser newUser)
        {
            if (newUser == null)
                return null;

            return new Managers.Users.NewUser(
                username: newUser.Username,
                password: newUser.Password,
                settings: newUser.Settings
                );
        }
    }
}
