using AceJobAgency.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace AceJobAgency.ViewModels
{
    public class MemberIdentity : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;

        // Encrypted
        public string Nric { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Resume { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;

        // Password History
        public List<PasswordHistory> PasswordHistory { get; set; } = new List<PasswordHistory>();
    }
}
