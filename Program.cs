using E_Learning.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using QuestPDF;
using QuestPDF.Infrastructure;
using Stripe;
using Microsoft.AspNetCore.Authentication;


var builder = WebApplication.CreateBuilder(args);

// Set QuestPDF license type
QuestPDF.Settings.License = LicenseType.Community;

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Add services to the container.
builder.Services.AddControllersWithViews();



builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

// Authentication Setup
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // Default to Google
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle(options =>
{
	options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
	options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
	options.CallbackPath = "/signin-google";
	options.ClaimActions.MapJsonKey("picture", "picture", "url"); // Get profile picture from Google
})
.AddFacebook(options =>
{
	options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
	options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
	options.CallbackPath = "/signin-facebook";
	options.SaveTokens = true;
});



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}



app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
	name: "educator",
	pattern: "Educator/{action=HomePage}/{id?}",
	defaults: new { controller = "Educator", action = "HomePage" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Roadmap}/{action=Index}/{id?}");


app.Run();
