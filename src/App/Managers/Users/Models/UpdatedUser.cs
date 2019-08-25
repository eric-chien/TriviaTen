namespace App.Managers.Users
{
    public class UpdatedUser
    {
        public string Username { get; set; }
        public UserSettings Settings { get; set; }

        public UpdatedUser(string username, UserSettings settings)
        {
            Username = username;
            Settings = settings;
        }
    }
}
