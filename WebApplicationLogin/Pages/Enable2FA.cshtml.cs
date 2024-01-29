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
	[Authorize(policy: "LoggedIn")]
	public class Enable2FAModel : PageModel
    {

		[BindProperty]
		public SecondFA VModel { get; set; }
		private readonly AuthDbContext _context;

		private readonly SignInManager<MemberIdentity> signInManager;
		public Enable2FAModel(SignInManager<MemberIdentity> signInManager, AuthDbContext context)
		{
			this.signInManager = signInManager;
			_context = context;
		}

		public void OnPost()
		{
		}

		public async Task<IActionResult> OnGetAsync()
        {
			var user = await signInManager.UserManager.GetUserAsync(User);
			if (user == null)
			{
				return RedirectToPage("/Index");
			}

			user.TwoFactorEnabled = true;
			user.EmailConfirmed = true;
			await signInManager.UserManager.UpdateAsync(user);
			return RedirectToPage("/Index");
        }
    }
}
