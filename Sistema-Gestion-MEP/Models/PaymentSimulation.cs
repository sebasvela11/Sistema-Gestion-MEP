using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Gestion_MEP.Models
{
    public class PaymentSimulation
    {
        public int Id { get; set; }

        public string UserId { get; set; }      // <-- Identity user
        public int SpecialtyId { get; set; }
        public int TermId { get; set; }

        public decimal? AmountCRC { get; set; } // se toma de SpecialtyAccess.PriceCRC
        public string Status { get; set; }      // "Pending" | "Paid"
        public string Reference { get; set; }
        public DateTime? PaidAtUtc { get; set; }
    }
}
