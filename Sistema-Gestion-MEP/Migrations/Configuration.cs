using System.Data.Entity.Migrations;
using Sistema_Gestion_MEP.Models;

namespace Sistema_Gestion_MEP.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {

        }
    }
}
