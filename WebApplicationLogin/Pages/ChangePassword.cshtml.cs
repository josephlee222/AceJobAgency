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
	[Authorize(Policy = "LoggedIn")]
	public class ChangePasswordModel : PageModel
    {

		[BindProperty]
		public ChangePassword CPModel { get; set; }
		private readonly AuthDbContext _context;

		private readonly SignInManager<MemberIdentity> signInManager;
		public ChangePasswordModel(SignInManager<MemberIdentity> signInManager, AuthDbContext context)
		{
			this.signInManager = signInManager;
			_context = context;
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{

				// Check the CAPTCHA
				if (!ValidateCaptcha())
				{
					ModelState.AddModelError("", "You failed the CAPTCHA");
					return Page();
				}

				// Get the user
				var user = await signInManager.UserManager.FindByEmailAsync(User.Identity.Name);
				var passwordHistory = await _context.PasswordHistories.Where(x => x.UserId == user.Id).ToListAsync();
				if (user == null)
				{
					ModelState.AddModelError("", "User not found");
					return Page();
				}

				// Check whether the old password is correct
				var result = await signInManager.UserManager.CheckPasswordAsync(user, CPModel.CurrentPassword);

				if (!result)
				{
					ModelState.AddModelError("", "Current password is incorrect");
					return Page();
				}

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

				// Check whether a password change happened in the last 24 hours
				var lastChange = passwordHistory.OrderByDescending(x => x.DateChanged).FirstOrDefault();
				if (lastChange != null)
				{
					if (lastChange.DateChanged.AddHours(24) > DateTime.Now)
					{
						ModelState.AddModelError("", "You cannot change your password more than once in 24 hours");
						return Page();
					}
				}

				// Change the password
				await signInManager.UserManager.ChangePasswordAsync(user, CPModel.CurrentPassword, CPModel.Password);

				// Add the password to the history
				user.PasswordHistory.Add(new PasswordHistory
				{
					UserId = user.Id,
					Password = signInManager.UserManager.PasswordHasher.HashPassword(user, CPModel.Password),
					DateChanged = DateTime.Now
				});

				// Add audit record
				_context.AuditLogs.Add(new AuditLog
				{
					UserId = user.Id,
					Activity = "Password Changed",
					CreatedDate = DateTime.Now
				});

				// Save the changes
				await signInManager.UserManager.UpdateAsync(user);
				await _context.SaveChangesAsync();
				
				// Return to the index page
				return RedirectToPage("Index");
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
			if (!signInManager.IsSignedIn(User)) { Response.Redirect("/Login"); }
        }
    }
}
