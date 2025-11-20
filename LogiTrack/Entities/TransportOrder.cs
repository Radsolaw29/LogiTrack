namespace LogiTrack.Entities
{
    public class TransportOrder
    {

        public int Id { get; set; }
        public string OrderName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public int PickupAddressId { get; set; }
        public virtual Address PickupAddress { get; set; }

        public int DeliveryAddressId { get; set; }
        public virtual Address DeliveryAddress { get; set; }

        public int  DriverId { get; set; }
        public virtual Driver Driver { get; set; }

        public int? TruckId { get; set; }
        public virtual Truck Truck { get; set; }

    }
}
