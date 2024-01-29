namespace AceJobAgency.ViewModels
{
	public class CapRes
	{
		public Boolean success { get; set; }
		public string challenge_ts { get; set; } = string.Empty;
		public string hostname { get; set; } = string.Empty;
		public string error_codes { get; set; } = string.Empty;
	}
}
