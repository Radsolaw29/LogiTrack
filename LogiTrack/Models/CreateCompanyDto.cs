using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class CreateCompanyDto
    {
        [Required]
        [MaxLength(25)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public int TaxNumber { get; set; }
        public int PhoneNumber { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        [MaxLength(50)]
        public string City { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Street { get; set; } = string.Empty;
        [Required]
        [MaxLength(12)]
        public string PostalCode { get; set; } = string.Empty;

    }
}
