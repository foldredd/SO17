using System.Security.Cryptography;
using System.Text;
using ApiServiceMVC.Models;
namespace ApiServiceMVC.Web.Api.Infrastructure.EncryptAndDecrypt {
    public class Encrypt {

        public static AuthorizationData EncryptPassword(string password) {
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            
            // Hashing using BCrypt
            string bcryptHash = BCrypt.Net.BCrypt.HashPassword(salt+password, 10);
            Console.WriteLine($"Salt: {salt}");
            Console.WriteLine($"BcryptHash: {bcryptHash}");
            // Создаем объект AuthorizationData и устанавливаем свойства
            AuthorizationData authorizationData = new AuthorizationData();
            authorizationData.Salt = Encoding.UTF8.GetBytes(salt);
            authorizationData.PasswordHash = Encoding.UTF8.GetBytes(bcryptHash);

            return authorizationData;
        }
    }
}
