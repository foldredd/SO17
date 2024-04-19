namespace ApiServiceMVC.Models {


    //a superclass was created for further expansion 
    public class User : BaseUser {
           
            public byte[] PasswordHash { get; set; }
            public byte[] PasswordSalt { get; set; }

            public int RoleId { get; set; }
            public Role? Role { get; set; }
      
     
}
        

}
