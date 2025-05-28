using System.Collections.Generic;

namespace DrawingApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;  

       
        public ICollection<Drawing> Drawings { get; set; } = new List<Drawing>();
    }
}
