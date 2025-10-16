using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Gestion_MEP.Models
{
    // Type: 1=Template, 2=Submission
    public class Document
    {
        public int Id { get; set; }

        public byte Type { get; set; }                 // 1/2
        public string OwnerUserId { get; set; }        // NULL en plantillas
        public int SpecialtyId { get; set; }
        public int TermId { get; set; }

        [Required, StringLength(255)]
        public string FileName { get; set; }

        [Required, StringLength(500)]
        public string StoredPath { get; set; }         // ruta relativa en App_Data

        public long? FileSizeBytes { get; set; }
        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? DeadlineUtc { get; set; }

        // Relaciones
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual Specialty Specialty { get; set; }
        public virtual Term Term { get; set; }
    }
}
