using _222726Y.Models;
using _222726Y.ViewModels;
using _222726Y.ViewModels.GoogleCaptcha;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.Lockout.AllowedForNewUsers = true; // Enable lockout for new users
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Lockout duration
	options.Lockout.MaxFailedAccessAttempts = 3;
}).AddEntityFrameworkStores<AuthDbContext>();

//google recaptcha
builder.Services.Configure<GoogleCaptchaConfig>(builder.Configuration.GetSection("GoogleReCaptcha"));
builder.Services.AddTransient(typeof(GoogleCaptchaService));
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
var cookieName = config["Authentication:CookieName"];


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = cookieName;
    options.ExpireTimeSpan = TimeSpan.FromSeconds(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Login";
    options.Events.OnRedirectToLogout = context =>
    {
        // Retrieve the current user
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = userManager.GetUserAsync(context.HttpContext.User).Result;
        Console.WriteLine("user", user);
        // If the user exists and is logged in, update the LoggedIn property
        if (user != null && user.LoggedIn)
        {
            user.LoggedIn = false;
            userManager.UpdateAsync(user).Wait();
        }

        // Redirect to the specified logout path
        return Task.CompletedTask;
    };

});
builder.Services.AddAuthentication(cookieName).AddCookie(cookieName, options =>
{
	options.Events.OnRedirectToLogin = context =>
	{
		// Handle the redirect logic here
		context.Response.Redirect("/Login"); 
		return Task.CompletedTask;
	};
	options.Cookie.Name = cookieName;
    options.AccessDeniedPath = "/Account/AccessDenied";
});



builder.Services.AddAuthorization(options =>
{
   
    options.AddPolicy("MustBelongToHRDepartment",
        policy => policy.RequireClaim("Department", "HR"));
});


//session

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddDistributedMemoryCache(); //save session in memory
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
});
//end of session

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseStatusCodePagesWithRedirects("/errors/custom{0}");

app.UseAuthorization();

app.MapRazorPages();

app.Run();
