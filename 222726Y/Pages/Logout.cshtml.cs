using _222726Y.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace _222726Y.Pages
{
    public class LogoutModel : PageModel
    {
		private readonly IHttpContextAccessor contxt;
        private readonly IConfiguration _configuration;
        private UserManager<ApplicationUser> userManager { get; }
        private readonly SignInManager<ApplicationUser> signInManager;
		public LogoutModel(IHttpContextAccessor httpContextAccessor, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
		{
            this.userManager = userManager;
            contxt = httpContextAccessor;
			this.signInManager = signInManager;
            _configuration = configuration;

        }
        public void OnGet()
        {
        }
		public async Task<IActionResult> OnPostLogoutAsync()
		{
            var sessionId = _configuration["SessionID:SessionIDConfig"];
            var jsonUserData = Request.Cookies[sessionId];

            var userFromSession = JsonConvert.DeserializeObject<ApplicationUser>(jsonUserData);
            var userEmail = userFromSession.Email;
            var allUsers = userManager.Users;
            var specificUser = (ApplicationUser)null;
            foreach (var user in allUsers)
            {
                if (user.Email == userEmail)
                {
                    Console.WriteLine("Found email");
                    specificUser = user;
                    break;
                }
            }
            if (specificUser.LoggedIn)
            {
                specificUser.LoggedIn = false;
                await userManager.UpdateAsync(specificUser);
            }
            else
            {
                Console.WriteLine("User is not logged in.");
            }
            await signInManager.SignOutAsync();
			contxt.HttpContext.Session.Clear();
			
			return RedirectToPage("Login");
		}
		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}
	}
}
