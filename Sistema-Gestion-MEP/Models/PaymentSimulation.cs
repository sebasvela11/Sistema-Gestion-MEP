using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Gestion_MEP.Models
{
    public class PaymentSimulation
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int SpecialtyAccessId { get; set; }

        public int? DocumentId { get; set; }

        public string PaymentType { get; set; }

        public decimal? AmountCRC { get; set; }
        public string Status { get; set; }
        public string Reference { get; set; }
        public DateTime? PaidAtUtc { get; set; }

        public virtual SpecialtyAccess SpecialtyAccess { get; set; }
        public virtual Document Document { get; set; }
    }
}