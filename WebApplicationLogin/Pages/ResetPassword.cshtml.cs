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
	public class ResetPasswordModel : PageModel
    {

		[BindProperty]
		public ResetPassword CPModel { get; set; }
		private readonly AuthDbContext _context;

		private readonly SignInManager<MemberIdentity> signInManager;
		public ResetPasswordModel(SignInManager<MemberIdentity> signInManager, AuthDbContext context)
		{
			this.signInManager = signInManager;
			_context = context;
		}

		public async Task<IActionResult> OnPostAsync(string token, string UserId)
		{
			if (ModelState.IsValid)
			{
				if (token == null || UserId == null)
				{
					return RedirectToPage("/Index");
				}

				// Check the CAPTCHA
				if (!ValidateCaptcha())
				{
					ModelState.AddModelError("", "You failed the CAPTCHA");
					return Page();
				}

				var user = await signInManager.UserManager.FindByIdAsync(UserId);

				if (user == null)
				{
					return RedirectToPage("/Index");
				}

				var passwordHistory = await _context.PasswordHistories.Where(x => x.UserId == user.Id).ToListAsync();

				// Check if the new password is the same as the old password
				var newResult = await signInManager.UserManager.CheckPasswordAsync(user, CPModel.Password);

				if (newResult)
				{
					ModelState.AddModelError("", "New password cannot be the same as the old password");
					return Page();
				}

				// Check the password is in the most recent 2 passwords
				var historyResult = passwordHistory.Take(2).OrderByDescending(x => x.DateChanged);

				// If the password is the last 2 passwords, return an error
				foreach (var item in historyResult)
				{
					var result2 = signInManager.UserManager.PasswordHasher.VerifyHashedPassword(user, item.Password, CPModel.Password);
					if (result2 == PasswordVerificationResult.Success)
					{
						ModelState.AddModelError("", "New password cannot be the same as the last 2 passwords");
						return Page();
					}
				}

				var result = await signInManager.UserManager.ResetPasswordAsync(user, token, CPModel.Password);

				if (result.Succeeded)
				{
					// Add the new password to the password history
					var newHistory = new PasswordHistory
					{
						UserId = user.Id,
						Password = CPModel.Password,
						DateChanged = DateTime.Now
					};

					// Create audit log
					var auditLog = new AuditLog
					{
						UserId = user.Id,
						CreatedDate = DateTime.Now,
						Activity = "Password reset"
					};

					_context.PasswordHistories.Add(newHistory);
					_context.AuditLogs.Add(auditLog);
					await _context.SaveChangesAsync();

					// Sign the user out
					await signInManager.SignOutAsync();

					return RedirectToPage("/Index");
				}
				else
				{
					return RedirectToPage("/Index");
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
		public async Task<IActionResult> OnGetAsync(string token, string UserId)
        {
			if (token == null || UserId == null)
			{
				return RedirectToPage("/Index");
			}

			var user = await signInManager.UserManager.FindByIdAsync(UserId);

			if (user == null)
			{
				return RedirectToPage("/Index");
			}

			return Page();
        }
    }
}
