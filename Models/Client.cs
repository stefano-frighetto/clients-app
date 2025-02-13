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
        public DateTime? Birthdate { get; set; }
        [Required]
        public string CUIT { get; set; }
        public string? Address { get; set; }
        [Required]
        [Phone]
        //[RegularExpression]
        public string CellPhone { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
