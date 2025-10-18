using System;

namespace Sistema_Gestion_MEP.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Entity { get; set; }
        public string EntityId { get; set; }
        public string Info { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public bool Success { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
