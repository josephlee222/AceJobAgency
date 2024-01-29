using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace AceJobAgency.ViewModels
{

    public class Register
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please enter your first name")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Please enter a valid first name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Please enter a valid last name")]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Please enter your NRIC")]
        [RegularExpression(@"^[STFG]\d{7}[A-Z]$", ErrorMessage = "Please enter a valid NRIC")]
        public string Nric { get; set; }

        [Required(ErrorMessage = "Please enter your date of birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Please upload your resume")]
        [DataType(DataType.Upload)]
        public IFormFile Resume { get; set; }

        [Required(ErrorMessage = "Please enter a short description about yourself")]
        [DataType(DataType.MultilineText)]
        public string About { get; set; }
    }

}