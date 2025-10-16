using System.Data.Entity;

namespace Sistema_Gestion_MEP.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection") { }

        public DbSet<Prueba> Pruebas { get; set; }  // tabla de prueba
    }

    public class Prueba
    {
        public int Id { get; set; }
        public string Texto { get; set; }
    }
}
