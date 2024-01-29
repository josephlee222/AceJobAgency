using System.ComponentModel.DataAnnotations;

namespace AceJobAgency.ViewModels
{
    public class ForgotPassword
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
