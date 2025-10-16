using System.Web.Mvc;
using Sistema_Gestion_MEP.Models;

public class HomeController : Controller
{
    public ActionResult TestDB()
    {
        using (var db = new ApplicationDbContext())
        {
            db.Pruebas.Add(new Prueba { Texto = "Hola SQL 2019 con Auth SQL" });
            db.SaveChanges();
        }
        return Content("OK DB");
    }
}
