namespace AceJobAgency.ViewModels
{
	public class AuditLog
	{
		public int Id { get; set; }
		public string UserId { get; set; } = string.Empty;
		public string Activity { get; set; } = string.Empty;
		public DateTime CreatedDate { get; set; }
	}
}
