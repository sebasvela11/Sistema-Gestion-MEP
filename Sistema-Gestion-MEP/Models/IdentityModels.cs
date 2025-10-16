using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sistema_Gestion_MEP.Models
{
    // Representa a los usuarios del sistema (profesores, coordinadores)
    public class ApplicationUser : IdentityUser
    {
        // Campo adicional opcional (por ejemplo, nombre completo)
        public string FullName { get; set; }

        // Genera la identidad (cookie) del usuario autenticado
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    // Contexto principal de Entity Framework para Identity y todas las tablas personalizadas
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // ----------------------------
        // TABLAS PERSONALIZADAS DEL SISTEMA
        // ----------------------------

        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<SpecialtyAccess> SpecialtyAccesses { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<PaymentSimulation> PaymentSimulations { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
    }
}
