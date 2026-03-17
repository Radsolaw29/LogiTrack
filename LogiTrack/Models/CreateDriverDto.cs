using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class CreateDriverDto
    {
        [Required]
        [MaxLength(25)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [MaxLength(11)]
        public string PersonalNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string LicenseDriving { get; set; } = string.Empty;
        public int? PhoneNumber { get; set; }
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        public int? CompanyId { get; set; }
    }
}
