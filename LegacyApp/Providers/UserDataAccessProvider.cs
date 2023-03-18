using LegacyApp.Models;

namespace LegacyApp.Providers
{
    public class UserDataAccessProvider : IUserDataAccessProvider
    {
        public void AddUser(User user)
        {
            UserDataAccess.AddUser(user);
        }
    }
}
