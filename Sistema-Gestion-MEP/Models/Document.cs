using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Gestion_MEP.Models
{
    public class Document
    {
        public int Id { get; set; }

        public int Type { get; set; }

        public string OwnerUserId { get; set; }

        public int SpecialtyAccessId { get; set; }

        public string FileName { get; set; }
        public string StoredPath { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAtUtc { get; set; }
        public DateTime? DeadlineUtc { get; set; }

        public decimal? PriceCRC { get; set; }

        public virtual SpecialtyAccess SpecialtyAccess { get; set; }
    }
}