namespace LogiTrack.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TaxNumber { get; set; }
        public int PhoneNumber { get; set; }
        public string ContactEmail { get; set; } = string.Empty;

        public int? CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }

        public int AddressId { get; set; }
        public virtual Address Address { get; set; }
        public virtual List<TransportOrder> TransportOrders { get; set; } = new();
        public virtual List<Driver> Drivers { get; set; } = new();
        public virtual List<Truck> Trucks { get; set; } = new();

    }
}
