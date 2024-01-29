using Microsoft.AspNetCore.Identity;
using WebApp_Core_Identity.Model;
using AceJobAgency.ViewModels;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddIdentity<MemberIdentity, IdentityRole>(options =>
{
	options.Lockout.MaxFailedAccessAttempts = 3;
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.AllowedForNewUsers = true;

	options.Password.RequireDigit = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequiredLength = 12;

	options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(5);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
	options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.ConfigureApplicationCookie(Config =>
{
	Config.ExpireTimeSpan = TimeSpan.FromMinutes(5);
	Config.LoginPath = "/Login";
	Config.Cookie.HttpOnly = true;
	Config.SlidingExpiration = true;
	Config.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth",options =>
{
    options.Cookie.Name = "MyCookieAuth";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBelongToHRDepartment", policy => policy.RequireClaim("Department", "HR"));
	options.AddPolicy("LoggedIn", policy => policy.RequireAssertion(context => true));
});

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

app.UseAuthorization();

app.UseAuthentication();

app.MapRazorPages();

app.Run();
