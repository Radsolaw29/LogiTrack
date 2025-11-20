namespace LogiTrack.Entities
{
    public class Address
    {

        public int Id { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;

        public virtual Company Company { get; set; }
        public virtual List<TransportOrder> PickupOrders { get; set; } = new();
        public virtual List<TransportOrder> DeliveryOrders { get; set; } = new();

    }
}
