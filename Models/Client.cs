using System.ComponentModel.DataAnnotations;

namespace ClientApi.Models
{
    public class Client
    {
        public int ClientId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }


        [Required]
        public string CorporateName { get; set; }

        [Required]
        [RegularExpression(@"^\d{2}-\d{8}-\d$", ErrorMessage = "Invalid CUIT. Must be XX-XXXXXXXX-X")]
        public string CUIT { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid phone number. Must be 10 consecutive numbers only.")]
        public string CellPhone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}