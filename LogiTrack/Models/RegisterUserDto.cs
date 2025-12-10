using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class RegisterUserDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        public int RoleId { get; set; } = 1;
    }
}
