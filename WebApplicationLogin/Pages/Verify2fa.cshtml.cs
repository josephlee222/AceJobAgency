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
using Microsoft.AspNetCore.Authorization;

namespace AceJobAgency.Pages
{
	[ValidateAntiForgeryToken]
	public class Verify2FAModel : PageModel
    {

		[BindProperty]
		public SecondFA VModel { get; set; }
		private readonly AuthDbContext _context;

		private readonly SignInManager<MemberIdentity> signInManager;
		public Verify2FAModel(SignInManager<MemberIdentity> signInManager, AuthDbContext context)
		{
			this.signInManager = signInManager;
			_context = context;
		}

		public async Task<IActionResult> OnPostAsync(string Email)
		{
			if (ModelState.IsValid)
			{
				if (VModel.Code == null)
				{
					return RedirectToPage("/Index");
				}

				// Check the CAPTCHA
				if (!ValidateCaptcha())
				{
					ModelState.AddModelError("", "You failed the CAPTCHA");
					return Page();
				}

				// Verify the 2FA code
				var result = await signInManager.TwoFactorSignInAsync("Email", VModel.Code, false, false);
				if (result.Succeeded)
				{
					// Get the user
					var user = await signInManager.UserManager.FindByEmailAsync(Email);

					// Create GUID
					var guid = Guid.NewGuid().ToString();
					user.GUID = guid;
					await signInManager.UserManager.UpdateAsync(user);

					//Create the security context
					var claims = new List<Claim> {
						new Claim(ClaimTypes.Name, user.Email),
						new Claim(ClaimTypes.Email, user.Email),
						new Claim("Department", "HR"),
					};

					Response.Cookies.Append("GUID", guid, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Strict,
					});

					var i = new ClaimsIdentity(claims, "MyCookieAuth");
					ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);
					await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

					// Create audit record
					var audit = new AuditLog
					{
						UserId = user.Id,
						CreatedDate = DateTime.Now,
						Activity = "Logged in via 2FA"
					};

					_context.AuditLogs.Add(audit);
					await _context.SaveChangesAsync();
					return RedirectToPage("/Index");
				}
				else
				{
					ModelState.AddModelError("", "Invalid code");
					return Page();
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
