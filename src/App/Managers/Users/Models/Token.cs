namespace App.Managers.Users
{
    public class Token
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public string FailureReason { get; set; }
    }
}
