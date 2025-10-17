using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Sistema_Gestion_MEP.Models;
using System.Data.Entity.Migrations;

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
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // 🔹 Crear roles si no existen
            string[] roles = { "Coordinador", "Profesor" };
            foreach (var r in roles)
            {
                if (!roleManager.RoleExists(r))
                    roleManager.Create(new IdentityRole(r));
            }

            // 🔹 Usuario administrador
            var adminEmail = "coordinador@mep.local";
            var admin = userManager.FindByEmail(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Admin MEP"
                };

                userManager.Create(admin, "Admin#2025");
                userManager.AddToRole(admin.Id, "Coordinador");
            }

            // 🔹 Usuario profesor
            var profesorEmail = "profesor@mep.local";
            var profesor = userManager.FindByEmail(profesorEmail);
            if (profesor == null)
            {
                profesor = new ApplicationUser
                {
                    UserName = profesorEmail,
                    Email = profesorEmail,
                    FullName = "Profesor MEP"
                };

                userManager.Create(profesor, "Profesor#2025");
                userManager.AddToRole(profesor.Id, "Profesor");
            }

            // ✅ Guardar cambios
            context.SaveChanges();
        }
    }
}
