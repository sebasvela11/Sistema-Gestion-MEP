using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Sistema_Gestion_MEP.Startup))]
namespace Sistema_Gestion_MEP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app) => ConfigureAuth(app);
    }
}
