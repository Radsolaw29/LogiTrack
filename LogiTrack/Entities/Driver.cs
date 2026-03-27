namespace LogiTrack.Entities
{
    public class Driver
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PersonalNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string LicenseDriving { get; set; } = string.Empty;
        public int PhoneNumber { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public int? CreatedById { get; set; }
        public virtual User? CreatedBy { get; set; }


        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public virtual List<TransportOrder> TransportOrders { get; set; } = new();
    }
}