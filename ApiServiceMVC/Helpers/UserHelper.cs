using ApiServiceMVC.DatabaseEnty;
using ApiServiceMVC.Models;

namespace ApiServiceMVC.Helpers
{
    public class UserHelper : Helper
    {
        public UserHelper(Database db) : base(db)
        {
        }
        public User FoundUser(string login)
        {


            using (Database db = new Database())
            {
                var authorizationData = (from user in db.Users
                                         where user.Login != null && user.Login == login
                                         select user).FirstOrDefault();
                return authorizationData;
            }
        }
    }
}
