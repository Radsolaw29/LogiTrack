namespace LogiTrack.Models
{
    public class DriverDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string LicenseDriving { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }
}
