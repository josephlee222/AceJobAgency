using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp_Core_Identity.Model;
using AceJobAgency.ViewModels;

namespace AceJobAgency.Pages
{
    public class LogoutModel : PageModel
    {

		private readonly SignInManager<MemberIdentity> signInManager;
		private readonly AuthDbContext _context;

		public LogoutModel(SignInManager<MemberIdentity> signInManager, AuthDbContext context)
		{
			this.signInManager = signInManager;
			_context = context;
		}

		public void OnGet()
        {
        }

		public async Task<IActionResult> OnPostLogoutAsync()
		{
			var user = await signInManager.UserManager.GetUserAsync(User);
			var audit = new AuditLog
			{
				UserId = user.Id,
				CreatedDate = DateTime.Now,
				Activity = "Logout",
			};

			_context.AuditLogs.Add(audit);

			await _context.SaveChangesAsync();
			await signInManager.SignOutAsync();
			return RedirectToPage("Login");
		}

		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}
	}
}
