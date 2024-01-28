using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace _222726Y.ViewModels
{
    public class ApplicationUser : IdentityUser
    {

        public string First_Name { get; set; }


        public string Last_Name { get; set; }
        public string Gender { get; set; }

        public string NRIC { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public DateTime DOB { get; set; }
        public String Resume { get; set; }
        public string WhoAmI { get; set; }
        public Boolean LoggedIn { get; set; }
    }

}
