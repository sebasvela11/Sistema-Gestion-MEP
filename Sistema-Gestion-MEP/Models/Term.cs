using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Gestion_MEP.Models
{
    public class Term
    {
        public int Id { get; set; }

        [Range(2000, 2100)]
        public int Year { get; set; } = DateTime.UtcNow.Year;

        [Required, StringLength(50)]
        public string Label { get; set; }   // "I Trimestre", "Semestre 2", etc.

        [Range(1, 4)]
        public byte OrderInYear { get; set; } = 1;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
