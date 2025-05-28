using System;
using System.Text.Json.Serialization;

namespace DrawingApp.Models
{
    public class Drawing
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string GeoJson { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }

        [JsonIgnore]
        public User Owner { get; set; } = null!;
    }
}
