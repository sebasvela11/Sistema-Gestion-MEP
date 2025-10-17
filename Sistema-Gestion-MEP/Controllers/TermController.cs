using System.Linq;
using System.Web.Mvc;
using Sistema_Gestion_MEP.Models;

namespace Sistema_Gestion_MEP.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class TermController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Term/List  (filtros opcionales)
        public ActionResult List(int? year, string q)
        {
            var data = db.Terms.AsQueryable();

            if (year.HasValue)
                data = data.Where(t => t.Year == year.Value);

            if (!string.IsNullOrWhiteSpace(q))
                data = data.Where(t => t.Label.Contains(q));

            var list = data
                .OrderBy(t => t.Year)
                .ThenBy(t => t.OrderInYear)
                .ToList();

            ViewBag.Year = year;
            ViewBag.Query = q;
            ViewBag.Total = list.Count;

            // Años disponibles para filtro
            ViewBag.Years = new SelectList(
                db.Terms.Select(t => t.Year).Distinct().OrderBy(y => y).ToList()
            );

            return View(list);
        }

        // GET: /Term/Create
        public ActionResult Create()
        {
            return View(new Term { Year = System.DateTime.UtcNow.Year, OrderInYear = 1 });
        }

        // POST: /Term/Create
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Term model)
        {
            if (!ModelState.IsValid) return View(model);

            // Evitar duplicados por (Year + OrderInYear) o (Year + Label)
            bool exists = db.Terms.Any(t =>
                t.Year == model.Year &&
                (t.OrderInYear == model.OrderInYear || t.Label == model.Label));

            if (exists)
            {
                ModelState.AddModelError("", "Ya existe un periodo con ese Año y Orden/Etiqueta.");
                return View(model);
            }

            db.Terms.Add(model);
            db.SaveChanges();

            TempData["ok"] = "Periodo creado correctamente.";
            return RedirectToAction("List");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
