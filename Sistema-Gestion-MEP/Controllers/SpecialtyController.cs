using System.Linq;
using System.Web.Mvc;
using Sistema_Gestion_MEP.Models;

namespace Sistema_Gestion_MEP.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class SpecialtyController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Speciality/List
        public ActionResult List(string q = null)
        {
            var data = db.Specialties.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                data = data.Where(s => s.Name.Contains(q));

            var list = data.OrderBy(s => s.Name).ToList();
            ViewBag.Total = list.Count;
            ViewBag.Query = q;
            return View(list);
        }

        // GET: /Speciality/Create
        public ActionResult Create()
        {
            return View(new Specialty { IsActive = true });
        }

        // POST: /Speciality/Create
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Specialty model)
        {
            if (!ModelState.IsValid) return View(model);

            // Evitar duplicados por nombre (opcional)
            bool exists = db.Specialties.Any(s => s.Name == model.Name);
            if (exists)
            {
                ModelState.AddModelError("Name", "Ya existe una especialidad con ese nombre.");
                return View(model);
            }

            db.Specialties.Add(model);
            db.SaveChanges();

            TempData["ok"] = "Especialidad creada correctamente.";
            return RedirectToAction("List");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
