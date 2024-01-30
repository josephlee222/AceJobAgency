using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using AceJobAgency.ViewModels;
using WebApp_Core_Identity.Model;

namespace AceJobAgency.Pages
{
    public class IndexModel : PageModel
    {
		public string Resume { get; set; }
		private readonly ILogger<IndexModel> _logger;
		private SignInManager<MemberIdentity> signInManager { get; }
		private UserManager<MemberIdentity> userManager { get; }
		private AuthDbContext _context { get; }
		public IndexModel(
			UserManager<MemberIdentity> userManager, 
			SignInManager<MemberIdentity> signInManager, 
			ILogger<IndexModel> logger,
			AuthDbContext context
		)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			_context = context;
			_logger = logger;
		}

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Gender { get; set; }
		public string Nric { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string About { get; set; }
		public string ResumeFile { get; set; }
        public async Task<IActionResult> OnGet()
        {
			// Get GUID
			var GUID = Request.Cookies["GUID"];
			var p = DataProtectionProvider.Create("EncryptData");
			var dataProtect = p.CreateProtector("MySecretKey");
			var user = await userManager.GetUserAsync(User);

            if (user != null)
            {
				// Check GUID
				if (user.GUID != GUID)
				{
					// GUID is not the same, audit and logout
					var audit = new AuditLog
					{
						CreatedDate = DateTime.Now,
						UserId = user.Id,
						Activity = "Logged out due to GUID mismatch"
					};

					_context.AuditLogs.Add(audit);
					await _context.SaveChangesAsync();
					await signInManager.SignOutAsync();
					return RedirectToPage("/Index");
				}

				// Check last password change
				var lastPasswordChange = user.LastPasswordChange;
				var minutesSinceLastPasswordChange = (DateTime.Now - lastPasswordChange).TotalMinutes;

				if (minutesSinceLastPasswordChange > 1)
				{
					return RedirectToPage("/ChangePassword");
				}


				Email = user.Email;
                FirstName = user.FirstName;
                LastName = user.LastName;
                Gender = user.Gender;
                Nric = dataProtect.Unprotect(user.Nric);
                DateOfBirth = user.DateOfBirth;
                ResumeFile = user.Resume;
				About = user.About;
			}

			return Page();
		}
    }
}