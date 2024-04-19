using ApiServiceMVC.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiServiceMVC.Models
{
    public class BaseUser:IFieldsFull {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }

        [NotMapped] // Это свойство не будет отображаться в базе данных
        public string Password { get; set; }
        public string Phone { get; set; }

        public bool IsFull() {
            if (this == null) return false;
            if (string.IsNullOrWhiteSpace(Email)) return false;
            if (string.IsNullOrWhiteSpace(Name)) return false;
            if (string.IsNullOrWhiteSpace(SurName)) return false;
            if (string.IsNullOrWhiteSpace(Phone)) return false;
            if (string.IsNullOrWhiteSpace(Password)) return false;
            if(string.IsNullOrWhiteSpace(Login)) return false;

            return true;
        }
    }

    
}
