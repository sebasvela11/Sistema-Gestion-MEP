using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Sistema_Gestion_MEP.Models;

namespace Sistema_Gestion_MEP
{
    public static class IdentitySeeder
    {
        public static void Seed()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                // Crear roles base
                string[] roles = { "Coordinador", "Profesor" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExists(role))
                        roleManager.Create(new IdentityRole(role));
                }

                // Crear usuario Coordinador
                if (userManager.FindByName("coordinador@mep.local") == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = "coordinador@mep.local",
                        Email = "coordinador@mep.local",
                        FullName = "Coordinador Técnico"
                    };
                    userManager.Create(user, "Admin#2025");
                    userManager.AddToRole(user.Id, "Coordinador");
                }

                // Crear usuario Profesor
                if (userManager.FindByName("profesor@mep.local") == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = "profesor@mep.local",
                        Email = "profesor@mep.local",
                        FullName = "Profesor de Prueba"
                    };
                    userManager.Create(user, "Profesor#2025");
                    userManager.AddToRole(user.Id, "Profesor");
                }

                context.SaveChanges();
            }
        }
    }
}
