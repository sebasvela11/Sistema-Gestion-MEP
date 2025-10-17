using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Gestion_MEP.Models
{
    public class Document
    {
        public int Id { get; set; }

        // 1 = Plantilla, 2 = Entrega
        public int Type { get; set; }

        // Para entrega: dueño del archivo (profesor)
        public string OwnerUserId { get; set; }    // <-- Identity user (solo para Type=2)
        public int SpecialtyId { get; set; }
        public int TermId { get; set; }

        public string FileName { get; set; }
        public string StoredPath { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAtUtc { get; set; }
        public DateTime? DeadlineUtc { get; set; } // opcional

        public virtual Specialty Specialty { get; set; }
        public virtual Term Term { get; set; }
    }
}
