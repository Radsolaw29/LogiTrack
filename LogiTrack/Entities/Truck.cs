namespace LogiTrack.Entities
{
    public class Truck
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Mileage { get; set; }
        public decimal CapacityTons { get; set; }
        public string Type { get; set; } = string.Empty;

        public int? CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }

        public virtual Company Company { get; set; }
        public virtual List<TransportOrder> TransportOrders { get; set; } = new();
    }
}