using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;            // ← Necesario para ctx.Get<T>()
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Sistema_Gestion_MEP.Models;

namespace Sistema_Gestion_MEP
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            // Crea el DbContext por request
            app.CreatePerOwinContext(ApplicationDbContext.Create);

            // Crea el UserManager por request
            app.CreatePerOwinContext<UserManager<ApplicationUser>>((options, context) =>
            {
                var store = new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()); // ← ahora compila
                var manager = new UserManager<ApplicationUser>(store);

                // (Opcional) reglas de contraseña
                manager.PasswordValidator = new PasswordValidator
                {
                    RequiredLength = 6,
                    RequireDigit = true
                };

                return manager;
            });

            // Autenticación por cookies
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                SlidingExpiration = true
            });
        }
    }
}
