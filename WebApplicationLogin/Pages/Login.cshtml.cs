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
				LModel.RememberMe, true);
				if (identityResult.IsLockedOut)
				{
					ModelState.AddModelError("", "Account locked out, try again later");
				} else if (identityResult.RequiresTwoFactor)
				{
					var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);
					var code = await signInManager.UserManager.GenerateTwoFactorTokenAsync(user, "Email");

					// Send email
					var client = new SmtpClient(
						"smtp.gmail.com",
						587
					);
					client.Credentials = new NetworkCredential("envirogo.noreply@gmail.com", "iygglhyrpdnvhtem");
					client.EnableSsl = true;

					var message = new MailMessage();
					message.From = new MailAddress("verify@acejobagency.com");
					message.To.Add(LModel.Email);
					message.Subject = "Verify 2-Factor Login";
					message.Body = $"Please use this code as your 2-factor authentication code: {code}";
					await client.SendMailAsync(message);

					return RedirectToPage("/Verify2FA", new { Email=LModel.Email });
				} else if (identityResult.Succeeded)
				{
					// Get the user
					var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);

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

					var audit = new AuditLog
					{
						UserId = user.Id,
						CreatedDate = DateTime.Now,
						Activity = "Login",
					};

					_context.AuditLogs.Add(audit);
					await _context.SaveChangesAsync();

					return RedirectToPage("Index");
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
