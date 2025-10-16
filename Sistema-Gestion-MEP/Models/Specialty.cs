using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Gestion_MEP.Models
{
    public class Specialty
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
