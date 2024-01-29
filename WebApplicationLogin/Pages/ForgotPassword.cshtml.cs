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
using System.Net.Mail;

namespace AceJobAgency.Pages
{
	[ValidateAntiForgeryToken]
	public class ForgotPasswordModel : PageModel
    {

		[BindProperty]
		public ForgotPassword FPModel { get; set; }
		private readonly AuthDbContext _context;

		private readonly SignInManager<MemberIdentity> signInManager;
		private readonly UserManager<MemberIdentity> userManager;
		public ForgotPasswordModel(SignInManager<MemberIdentity> signInManager, 
			AuthDbContext context,
			UserManager<MemberIdentity> userManager
			)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
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

				var user = userManager.FindByEmailAsync(FPModel.Email).Result;
				if (user == null)
				{
					return RedirectToPage("/Login");
				}

				var token = await userManager.GeneratePasswordResetTokenAsync(user);

				// Send email
				var client = new SmtpClient(
					"smtp.gmail.com",
					587
				);
				client.Credentials = new NetworkCredential("envirogo.noreply@gmail.com", "iygglhyrpdnvhtem");
				client.EnableSsl = true;

				// Make link
				var link = Url.Page("/ResetPassword", null, new { token, UserId=user.Id }, Request.Scheme, Request.Host.ToString());

				var message = new MailMessage();
				message.From = new MailAddress("reset@acejobagency.com");
				message.To.Add(FPModel.Email);
				message.Subject = "Reset Password";
				message.Body = $"Please click the link below to reset your password: {link}";
				await client.SendMailAsync(message);

				return RedirectToPage("/Login");
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
