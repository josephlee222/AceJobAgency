using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AceJobAgency.ViewModels;
using System.Text.Encodings.Web;

namespace AceJobAgency.Pages
{
	[ValidateAntiForgeryToken]
	public class RegisterModel : PageModel
    {
		private UserManager<MemberIdentity> userManager { get; }
		private SignInManager<MemberIdentity> signInManager { get; }
		[BindProperty]
		public Register RModel { get; set; }
		public RegisterModel(UserManager<MemberIdentity> userManager,
		SignInManager<MemberIdentity> signInManager)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
		}
		public void OnGet()
        {
        }

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				// Check ReCaptcha
				if (!ValidateCaptcha())
				{
					ModelState.AddModelError("", "You failed the CAPTCHA");
					return Page();
				}

				// Check if email is used
				var existingUser = await userManager.FindByEmailAsync(RModel.Email);
				if (existingUser != null)
				{
					ModelState.AddModelError("RModel.Email", "Unable to register, E-mail address is used.");
					return Page();
				}

				// Check if resume is in PDF or Microsoft Word format
				if (RModel.Resume != null)
				{
					Console.WriteLine(RModel.Resume.FileName);
					var ext = Path.GetExtension(RModel.Resume.FileName);
					if (!(ext == ".pdf" || ext == ".docx" || ext == ".doc"))
					{
						ModelState.AddModelError("RModel.Resume", "Resume must be in PDF or Microsoft Word format");
						return Page();
					}
				}	

				// Upload resume
				var fileName = Path.GetFileName(RModel.Resume.FileName);
				var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resume", RModel.Email);
				var filePath = Path.Combine(folderPath, fileName);
				Directory.CreateDirectory(folderPath);
				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await RModel.Resume.CopyToAsync(fileStream);
				}

				// Create data protection provider
				var p = DataProtectionProvider.Create("EncryptData");
				var dataProtect = p.CreateProtector("MySecretKey");
				var encoder = UrlEncoder.Create();

				//Create identity user
				var user = new MemberIdentity
				{
					UserName = RModel.Email,
					Email = RModel.Email,
					FirstName = RModel.FirstName,
					LastName = RModel.LastName,
					Gender = RModel.Gender,
					Nric = dataProtect.Protect(RModel.Nric),
					DateOfBirth = RModel.DateOfBirth,
					Resume = RModel.Resume.FileName,
					About = HtmlEncoder.Default.Encode(RModel.About),
				};
				var result = await userManager.CreateAsync(user, RModel.Password);
				if (result.Succeeded)
				{
					return RedirectToPage("Login");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
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
	}
}
