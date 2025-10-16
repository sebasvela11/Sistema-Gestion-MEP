using System;

namespace Sistema_Gestion_MEP.Models
{
    public class ActivityLog
    {
        public long Id { get; set; }
        public string UserId { get; set; }     // puede ser null (visitante)
        public string Action { get; set; }     // Login, Logout, Download, Upload, etc.
        public string Entity { get; set; }     // "Document", "Specialty", ...
        public string EntityId { get; set; }   // Id del recurso (string para flexibilidad)
        public string Info { get; set; }       // detalle adicional
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public bool Success { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser User { get; set; }
    }
}
