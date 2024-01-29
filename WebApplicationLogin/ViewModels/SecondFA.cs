using System.ComponentModel.DataAnnotations;

namespace AceJobAgency.ViewModels
{
    public class SecondFA
    {
        [Required]
        [DataType(DataType.Text)]
        public string Code { get; set; }
    }
}
