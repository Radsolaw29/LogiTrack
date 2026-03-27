using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class UpdateTruckDto
    {
        [Required]
        [MaxLength(11)]
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Mileage { get; set; }
        public decimal CapacityTons { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
