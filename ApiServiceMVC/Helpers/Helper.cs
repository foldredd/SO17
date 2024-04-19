using ApiServiceMVC.DatabaseEnty;

namespace ApiServiceMVC.Helpers
{
    public abstract class Helper
    {

        Database db { get; set; }
        public Helper(Database db)
        {
            this.db = db;
        }
    }
}
