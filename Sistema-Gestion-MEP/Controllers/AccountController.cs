using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Sistema_Gestion_MEP.Models;

namespace Sistema_Gestion_MEP.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        public UserManager<ApplicationUser> UserManager =>
            _userManager ?? HttpContext.GetOwinContext().GetUserManager<UserManager<ApplicationUser>>();

        private IAuthenticationManager Auth => HttpContext.GetOwinContext().Authentication;

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Si ya está autenticado, lo enviamos directo al menú principal
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await UserManager.FindAsync(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View(model);
            }

            var identity = await user.GenerateUserIdentityAsync(UserManager);

            // Reiniciar cookie y firmar
            Auth.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Auth.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe }, identity);

            // 1) Si venía de una URL interna, respetarla
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // 2) Enviar al MENÚ principal (Home/Index)
            return RedirectToAction("Index", "Home");

            // --- Variante por rol (si prefieres paneles distintos) ---
            // if (await UserManager.IsInRoleAsync(user.Id, "Coordinador"))
            //     return RedirectToAction("Index", "Coordinador");
            // return RedirectToAction("Index", "Profesor");
        }

        [Authorize]
        public ActionResult LogOff()
        {
            Auth.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }
    }

    public class LoginViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
