namespace LogiTrack.Models
{
    public class TransportOrderDto
    {

        public int Id { get; set; }
        public string OrderName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }

        public int CompanyId { get; set; }

    }
}
