using _222726Y.ViewModels;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace _222726Y.Pages
{

    public class VerificationModel : PageModel
    {
		private readonly IConfiguration _configuration;
		private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }
        private readonly RoleManager<IdentityRole> roleManager;
        [BindProperty]
        public Verification VModel { get; set; }
        public VerificationModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
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
        public async Task<IActionResult> OnPostAsync()
        {
            var cookieName = _configuration["Verification:CookieName"];
			var jsonUserData = Request.Cookies[cookieName];
            try
            {
                var user = JsonConvert.DeserializeObject<Dictionary<string, ApplicationUser>>(jsonUserData);

                if (user != null)
                {
                    if (ModelState.IsValid)
                    {
                        string code = "";
                        object users = null;
                        ApplicationUser userSaved = null;
						foreach (KeyValuePair<string, ApplicationUser> kvp in user)
						{

							code = kvp.Key;
							users = kvp.Value;
							// Your logic for each key-value pair
							Console.WriteLine($"Key: {userSaved}, Value: {code}");
                        }
                        if (VModel.code == int.Parse(code))
                        {
							userSaved = (ApplicationUser)users;
							var result = await userManager.CreateAsync(userSaved, userSaved.Password);
                            if (result.Succeeded)
                            {
                                await signInManager.SignInAsync(userSaved, false);
                                return RedirectToPage("Index");
                            }
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                        else
                        {
							ModelState.AddModelError("", "Wrong Code, Please try again");
						}
					}
                }
                else
                {
                    Console.WriteLine("Deserialization failed. User is null.");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error during deserialization: {ex.Message}");
            }
			return Page();

		}
	}
}
