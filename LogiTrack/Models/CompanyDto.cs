namespace LogiTrack.Models
{
    public class CompanyDto
    {

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TaxNumber { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;

        public List<TransportOrderDto> Orders { get; set; } = new();

        public List<DriverDto> Drivers { get; set; } = new();

        public List<TruckDto> Trucks { get; set; } = new();


    }
}
