namespace App.Managers.Users
{
    public class NewUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserSettings Settings { get; set; }

        public NewUser(string username, string password, UserSettings settings)
        {
            Username = username;
            Password = password;
            Settings = settings;
        }
    }
}
