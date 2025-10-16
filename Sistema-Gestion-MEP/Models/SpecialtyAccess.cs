using System;

namespace Sistema_Gestion_MEP.Models
{
    // Qué ve cada usuario: UserId ↔ Specialty ↔ Term
    public class SpecialtyAccess
    {
        public int Id { get; set; }

        public string UserId { get; set; }   // AspNetUsers.Id (string)
        public int SpecialtyId { get; set; }
        public int TermId { get; set; }

        public DateTime AccessGrantedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? DeadlineUtc { get; set; }

        // Relaciones
        public virtual ApplicationUser User { get; set; }
        public virtual Specialty Specialty { get; set; }
        public virtual Term Term { get; set; }
    }
}
