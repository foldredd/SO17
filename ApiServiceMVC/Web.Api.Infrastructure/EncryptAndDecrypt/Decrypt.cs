using System.Security.Cryptography;
using System.Text;

namespace ApiServiceMVC.Web.Api.Infrastructure.EncryptAndDecrypt {
    public class Decryption {
        public static bool VerifyPassword(string password, string hashedPassword) {
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"HashedPassword: {hashedPassword}");


            // Проверка с использованием BCrypt
            bool verification = BCrypt.Net.BCrypt.Verify(password, hashedPassword);

            // Вывод результата
            Console.WriteLine(verification);
           if(!verification) {
                Console.WriteLine("NOOO");
            }
            return verification;
        }

    }
}
