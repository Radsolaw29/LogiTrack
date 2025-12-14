namespace LogiTrack.Models
{
    public class CompanyQuery
    {
        public string SearchPhrase { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
