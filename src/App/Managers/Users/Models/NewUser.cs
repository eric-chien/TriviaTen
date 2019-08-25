namespace App.Managers.Users
{
    public class NewUser
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public NewUser(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
