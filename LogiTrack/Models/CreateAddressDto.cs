using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class CreateAddressDto
    {

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
