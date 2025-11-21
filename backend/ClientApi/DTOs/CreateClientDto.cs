using System.ComponentModel.DataAnnotations;

namespace ClientApi.DTOs
{
    public class CreateClientDto
    {
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        public required string CorporateName { get; set; }

        [Required]
        [RegularExpression(@"^\d{2}-\d{8}-\d$", ErrorMessage = "Invalid CUIT. Must be XX-XXXXXXXX-X")]
        public required string CUIT { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public required DateTime Birthdate { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid phone number. Must be 10 consecutive numbers only.")]
        public required string CellPhone { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
