using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
namespace _222726Y.ViewModels
{
    public class Register
    {
        [Required]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "Only letters are allowed.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last Name must be at least 2 characters long.")]
        public string First_Name { get; set; }
        [Required]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "Only letters are allowed.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First Name must be at least 2 characters long.")]

        public string Last_Name { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        [DataType(DataType.CreditCard)]
        [RegularExpression(@"^[A-Za-z]\d{7}[A-Za-z]$")]
        public string NRIC { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,}$", ErrorMessage = "Passwords must be at least 12 characters long and contain at least an uppercase letter, lower case letter, digit and a symbol")]
        [StringLength(100, MinimumLength = 12)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "The password must be at least 12 characters long.")]
        public string ConfirmPassword { get; set; }
        [Required]
		public DateTime DOB { get; set; }
		[Required(ErrorMessage = "Please select a file.")]
		[DataType(DataType.Upload)]
		[Display(Name = "File")]

		public string Resume { get; set; }
		[Required]
		[StringLength(400, MinimumLength = 12, ErrorMessage = "Who am I section must be at least 12 characters long.")]

		public String WhoAmI { get; set; }

        [Required]
        public Boolean LoggedIn { get; set; }


    }
}
