using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using AceJobAgency.ViewModels;

namespace AceJobAgency.Pages
{
    public class IndexModel : PageModel
    {
		public string Resume { get; set; }
		private readonly ILogger<IndexModel> _logger;
		private SignInManager<MemberIdentity> signInManager { get; }
		private UserManager<MemberIdentity> userManager { get; }
		public IndexModel(UserManager<MemberIdentity> userManager, SignInManager<MemberIdentity> signInManager, ILogger<IndexModel> logger)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
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
					// GUID is not the same, logout
					await signInManager.SignOutAsync();
					return RedirectToPage("/Index");
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