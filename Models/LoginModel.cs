using System.ComponentModel.DataAnnotations;

namespace DayByDay.Models
{
    public class Login
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}