using System;

namespace Sistema_Gestion_MEP.Models
{
    public class SpecialtyAccess
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public int SpecialtyId { get; set; }
        public int TermId { get; set; }

        public DateTime AccessGrantedUtc { get; set; }
        public DateTime? DeadlineUtc { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Specialty Specialty { get; set; }
        public virtual Term Term { get; set; }
    }
}
