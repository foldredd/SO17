namespace ApiServiceMVC.Models {
    public class AuthorizationData {
        public byte[] PasswordHash { get; set; }
        public byte[] Salt { get; set; }
    }
}
