using _222726Y.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using _222726Y.ViewModels;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;

namespace _222726Y.Pages
{
	public class RegisterModel : PageModel
	{
		public interface IEmailSender
		{
			void SendEmail(string toEmail, string subject);
		}

		private readonly IConfiguration _configuration;
		private UserManager<ApplicationUser> userManager { get; }
		private SignInManager<ApplicationUser> signInManager { get; }
		private readonly RoleManager<IdentityRole> roleManager;
		[BindProperty]
		public Register RModel { get; set; }
		public RegisterModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
		SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.roleManager = roleManager;
			_configuration = configuration;
		}
		public void OnGet()
		{


		}
		//Save data into the database
		public async Task<IActionResult> OnPostAsync()
		{
			var allowedExtensions = new[] { ".pdf", ".docx" };
			if (ModelState.IsValid)
			{
				
				var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
				var protector = dataProtectionProvider.CreateProtector("MySecretKey");

				// Validate file type
				if (allowedExtensions.Contains(Path.GetExtension(RModel.Resume).ToLowerInvariant()) == false)
				{
					ModelState.AddModelError("Resume", "Invalid file type. Only PDF and DOCX files are allowed.");
				}
				//date validation
				if (RModel.DOB >= DateTime.Today)
				{
					ModelState.AddModelError("DOB", "Invalid Birth Date");
				}

				/*var validEmail = await userManager.FindByEmailAsync(RModel.Email);

				if (validEmail != null)
				{
					ModelState.AddModelError("Email", "Email already in used");
				}*/
				//salt hash password 
				SHA512Managed hashing = new SHA512Managed();
				byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(RModel.Password));
				var pwHash = Convert.ToBase64String(hashWithSalt);


				// Proceed with user creation if validation passes
				var user = new ApplicationUser()
				{
					First_Name = RModel.First_Name,
					Last_Name = RModel.Last_Name,
					Gender = RModel.Gender,
					NRIC = protector.Protect(RModel.NRIC),
					UserName = RModel.Email,
					Email = RModel.Email,
					Password = pwHash,
                    ConfirmPassword = RModel.ConfirmPassword,
					DOB = RModel.DOB,
					Resume = RModel.Resume,
					WhoAmI = RModel.WhoAmI,
					LoggedIn = true
				};
				var code = GenerateRandomCode();
                var userData = new Dictionary<string, ApplicationUser>
                {
					{code.ToString(),user }
                };
				//put into cookie
                var jsonUserData = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
				Console.WriteLine(jsonUserData);
                var cookieOptions = new CookieOptions
                {
                    // Set the cookie expiration time
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    HttpOnly = true,
                    Secure = true,  
                    SameSite = SameSiteMode.Strict // Adjust based on your requirements
                };
				/*var cookieName = Guid.NewGuid().ToString();

				Environment.SetEnvironmentVariable("Verification_Cookie", cookieName);*/


				var cookieName = _configuration["Verification:cookieName"];
				Console.WriteLine(cookieName);
				Response.Cookies.Append(cookieName, jsonUserData, cookieOptions);
				SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
				client.EnableSsl = true;
				client.UseDefaultCredentials = false;

				var fromEmail = _configuration["Smtp:FromAddress"];
				var password = _configuration["Smtp:Password"];

				client.Credentials = new NetworkCredential(fromEmail, password);

				// Create email message
				MailMessage mailMessage = new MailMessage();
				mailMessage.From = new MailAddress(fromEmail);
				mailMessage.To.Add(RModel.Email);
				mailMessage.Subject = "Confirmation Code";
				mailMessage.IsBodyHtml = true;
				StringBuilder mailBody = new StringBuilder();
				mailBody.AppendFormat("<h1>User Registered</h1>");
				mailBody.AppendFormat("<br />");
				mailBody.AppendFormat("<p>Thank you For Registering account</p>");
				mailBody.AppendFormat("<p>This is your code: {0}</p>", code);
				mailMessage.Body = mailBody.ToString();

				// Send email
				try
				{
					client.Send(mailMessage);
					return RedirectToPage("Verification");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error sending email: {ex.Message}");
					ModelState.AddModelError("", "Unable to send code");
				}
				/*var result = await userManager.CreateAsync(user, pwHash);
				if (result.Succeeded)
					{
						await signInManager.SignInAsync(user, false);
						return RedirectToPage("Index");
					}
				foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}*/
			}
			return Page();
		}
        static int GenerateRandomCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999);
        }
    }
}

