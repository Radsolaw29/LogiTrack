using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Entities
{
    public class Role
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}