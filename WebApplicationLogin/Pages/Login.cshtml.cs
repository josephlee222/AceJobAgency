using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp_Core_Identity.Model;
using AceJobAgency.ViewModels;
using static System.Net.WebRequestMethods;

namespace AceJobAgency.Pages
{
    public class LoginModel : PageModel
    {

		[BindProperty]
		public Login LModel { get; set; }
		private readonly AuthDbContext _context;

		private readonly SignInManager<MemberIdentity> signInManager;
		public LoginModel(SignInManager<MemberIdentity> signInManager, AuthDbContext context)
		{
			this.signInManager = signInManager;
			_context = context;
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				if (!ValidateCaptcha())
				{
					ModelState.AddModelError("", "You failed the CAPTCHA");
					return Page();
				}

				var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password,
				LModel.RememberMe, false);
				if (identityResult.Succeeded)
				{
					// Get the user
					var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);

					//Create the security context
					var claims = new List<Claim> {
						new Claim(ClaimTypes.Name, user.Email),
						new Claim(ClaimTypes.Email, user.Email),
						new Claim("Department", "HR")
};
					var i = new ClaimsIdentity(claims, "MyCookieAuth");
					ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);
					await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

					var audit = new AuditLog
					{
						UserId = user.Id,
						CreatedDate = DateTime.Now,
						Activity = "Login",
					};

					_context.AuditLogs.Add(audit);
					await _context.SaveChangesAsync();

					return RedirectToPage("Index");
				} else if (identityResult.IsLockedOut)
				{
					ModelState.AddModelError("", "Account locked out, try again later");
				} else
				{
					ModelState.AddModelError("", "Username or Password incorrect");
				}
			}
			return Page();
		}

		public bool ValidateCaptcha()
		{
			var Response = Request.Form["g-recaptcha-response"];
			var valid = false;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=6LeFHl8pAAAAAAYEwyZx3Wlr1PNXx7zJpGfXwTyd&response=" + Response);
			try
			{
				using (WebResponse wResponse = request.GetResponse())
				{
					using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
					{
						string jsonResponse = readStream.ReadToEnd();
						var data = JsonSerializer.Deserialize<CapRes>(jsonResponse);
						valid = Convert.ToBoolean(data.success);
					}
				}
				return valid;
			}
			catch (WebException e)
			{
				throw e;
			}

		}
		public void OnGet()
        {
        }
    }
}
