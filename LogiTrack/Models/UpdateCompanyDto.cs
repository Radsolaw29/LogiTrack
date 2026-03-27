using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class UpdateCompanyDto
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(25)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public int TaxNumber { get; set; }
        public string ContactEmail { get; set; } = string.Empty;

    }
}
