namespace App.Controllers.V1.Users.Models
{
    public class CreatedUser
    {
        public User User { get; set; }
        public string ErrorMessage { get; set; }

        public static CreatedUser Convert(Managers.Users.CreatedUser createdUser)
        {
            if (createdUser == null)
                return null;

            return new CreatedUser
            {
                User = User.Convert(createdUser.User),
                ErrorMessage = createdUser.ErrorMessage
            };
        }
    }
}
