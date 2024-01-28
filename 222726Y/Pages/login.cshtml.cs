using _222726Y.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using AspNetCore.ReCaptcha;
using _222726Y.ViewModels.GoogleCaptcha;

namespace _222726Y.Pages
{
	public class LoginModel : PageModel
	{
		private readonly IConfiguration _configuration;
		private readonly GoogleCaptchaService _captchaService;
        private readonly IHttpContextAccessor contxt;

        [BindProperty]

		public Login LModel { get; set; }

		private UserManager<ApplicationUser> userManager { get; }

		private readonly SignInManager<ApplicationUser> signInManager;
		public LoginModel(IHttpContextAccessor httpContextAccessor,SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration, GoogleCaptchaService captchaService)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			_configuration = configuration;
			_captchaService = captchaService;
            contxt = httpContextAccessor;

        }
        public void OnGet()
		{
		}
		public async Task<IActionResult> OnPostAsync()
		{
			var cookieName = _configuration["Authentication:CookieName"];
			//use recaptcha
			var captchaResult = await _captchaService.VerifyToken(LModel.Token);
			if (!captchaResult)
			{
				Console.WriteLine("Captcha Result dont match");
				return Page();
			}
			if (ModelState.IsValid)
			{


				SHA512Managed hashing = new SHA512Managed();
				byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(LModel.Password));
				var pwHash = Convert.ToBase64String(hashWithSalt);

				var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, pwHash,
				LModel.RememberMe, false);
                var specificUser = (ApplicationUser)null;
				var allUsers = userManager.Users;
				foreach (var user in allUsers)
				{
					if (user.Email == LModel.Email)
					{
						Console.WriteLine("Found email");
						specificUser = user;
						break;
					}
				}

                Console.WriteLine(specificUser);
				if (specificUser == null) {
					Console.WriteLine("cannot find email");
                    ModelState.AddModelError("", "Username or Password incorrect");
					return Page();
                }

				if (specificUser.LoggedIn == true)
				{
					Console.WriteLine("user is logged in");
					ModelState.AddModelError("", "Already Logged in");
					return Page();

				}


				if (await userManager.IsLockedOutAsync(specificUser))
					{
						ModelState.AddModelError("", "Account associated to this email is locked");
						return Page();	
					}
				if (identityResult.Succeeded)
				{

					//create the security context
					var claims = new List<Claim>
					{
					new Claim(ClaimTypes.Name,specificUser.First_Name + specificUser.Last_Name),
					new Claim(ClaimTypes.Email,LModel.Email),

					new Claim("Department","HR")
					};
					var i = new ClaimsIdentity(claims, cookieName);
					ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);


					Console.WriteLine("loggingin");
                    specificUser.LoggedIn = true;
                    await userManager.UpdateAsync(specificUser);

                    var User = JsonConvert.SerializeObject(specificUser);
					var sessionid = _configuration["SessionID:SessionIDConfig"];
                    var cookieOptions = new CookieOptions
                    {
                        // Set the cookie expiration time
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict // Adjust based on your requirements
                    };

					Response.Cookies.Append(sessionid, User, cookieOptions);
                    await HttpContext.SignInAsync(cookieName, claimsPrincipal);
					return RedirectToPage("Index");

					
					

					/*string guid = Guid.NewGuid().ToString();*/

					//Response.Cookies.Add(new HttpCookie("AuthToken",guid));

				}
				else
				{
                Console.WriteLine("pass wrong");
                await userManager.AccessFailedAsync(specificUser);
					ModelState.AddModelError("", "Username or Password incorrect");
				}
			}
			
			return Page();
		}
	}

}
