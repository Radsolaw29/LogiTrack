using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class CreateTransportOrderDto
    {
        [Required]
        [MaxLength(100)]
        public string OrderName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }

        //[Required]
        ////public int CompanyId { get; set; }
        [Required]
        public int PickupAddressId { get; set; }
        [Required]
        public int DeliveryAddressId { get; set; }
        [Required]
        public int DriverId { get; set; }
        [Required]
        public int? TruckId { get; set; }
    }
}
