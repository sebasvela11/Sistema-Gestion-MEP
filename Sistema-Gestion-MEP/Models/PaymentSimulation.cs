using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Gestion_MEP.Models
{
    public class PaymentSimulation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int SpecialtyId { get; set; }
        public int TermId { get; set; }

        [Range(0, 999999999)]
        public decimal AmountCRC { get; set; } = 0;

        [StringLength(20)]
        public string Status { get; set; } = "Pending";  // 'Pending' | 'Paid'

        [StringLength(50)]
        public string Reference { get; set; }            // p.ej. "SIM-2025-0001"
        public DateTime? PaidAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser User { get; set; }
        public virtual Specialty Specialty { get; set; }
        public virtual Term Term { get; set; }
    }
}
