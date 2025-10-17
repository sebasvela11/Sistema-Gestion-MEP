using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Gestion_MEP.Models
{
    public class Term
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string Label { get; set; }      // "I Trimestre", "II Semestre", etc.
        public int OrderInYear { get; set; }   // 1,2,3...
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
