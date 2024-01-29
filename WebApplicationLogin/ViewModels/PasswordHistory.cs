namespace AceJobAgency.ViewModels
{
	public class PasswordHistory
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public string Password { get; set; }
		public DateTime DateChanged { get; set; }
	}
}
