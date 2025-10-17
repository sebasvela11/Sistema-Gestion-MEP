using System;
using System.Linq;
using System.Web.Mvc;
using Sistema_Gestion_MEP.Models;

namespace Sistema_Gestion_MEP.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class AccessController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // ============ LIST ============
        // GET: /Access/List  (con filtros opcionales)
        public ActionResult List(int? specialtyId, int? termId)
        {
            var query = db.SpecialtyAccesses.AsQueryable();

            if (specialtyId.HasValue)
                query = query.Where(a => a.SpecialtyId == specialtyId.Value);

            if (termId.HasValue)
                query = query.Where(a => a.TermId == termId.Value);

            var accesos = query
                .Select(a => new AccessListViewModel
                {
                    Id = a.Id,
                    Profesor = a.User.Email,
                    Especialidad = a.Specialty.Name,
                    Periodo = a.Term.Label + " " + a.Term.Year,
                    FechaAcceso = a.AccessGrantedUtc,
                    FechaLimite = a.DeadlineUtc,

                    // Para acciones por fila
                    SpecialtyId = a.SpecialtyId,
                    TermId = a.TermId,

                    // NUEVO: mostrar precio del acceso
                    PriceCRC = a.PriceCRC
                })
                .OrderByDescending(a => a.FechaAcceso)
                .ToList();

            // Combos de filtro
            ViewBag.SpecialtyId = new SelectList(db.Specialties.OrderBy(s => s.Name), "Id", "Name", specialtyId);

            var terms = db.Terms
                .OrderBy(t => t.Year).ThenBy(t => t.OrderInYear)
                .Select(t => new { t.Id, Label = t.Year + " - " + t.Label })
                .ToList();
            ViewBag.TermId = new SelectList(terms, "Id", "Label", termId);

            ViewBag.Total = accesos.Count;
            return View(accesos);
        }

        // ============ CREATE ============
        // GET: /Access/Create
        public ActionResult Create()
        {
            PopulateSelects();
            // PriceCRC se puede dejar null (gratis) o con un valor en colones
            return View(new SpecialtyAccess());
        }

        // POST: /Access/Create
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(SpecialtyAccess model)
        {
            if (model.PriceCRC.HasValue && model.PriceCRC.Value < 0)
                ModelState.AddModelError("PriceCRC", "El precio no puede ser negativo.");

            if (!ModelState.IsValid)
            {
                PopulateSelects(model.UserId, model.SpecialtyId, model.TermId);
                return View(model);
            }

            // Evitar duplicados: mismo profesor + especialidad + periodo
            bool exists = db.SpecialtyAccesses.Any(a =>
                a.UserId == model.UserId &&
                a.SpecialtyId == model.SpecialtyId &&
                a.TermId == model.TermId);

            if (exists)
            {
                ModelState.AddModelError("", "Ya existe un acceso para ese profesor en esa especialidad y periodo.");
                PopulateSelects(model.UserId, model.SpecialtyId, model.TermId);
                return View(model);
            }

            // Guardar: si PriceCRC es null o 0 => descarga libre
            model.AccessGrantedUtc = DateTime.UtcNow;
            db.SpecialtyAccesses.Add(model);
            db.SaveChanges();

            TempData["ok"] = "Acceso creado correctamente.";
            return RedirectToAction("List");
        }

        // ============ HELPERS ============
        private void PopulateSelects(string userId = null, int? specialtyId = null, int? termId = null)
        {
            // Profesores (usuarios con rol 'Profesor')
            var roleProf = db.Roles.FirstOrDefault(r => r.Name == "Profesor");
            var profesores = roleProf == null
                ? db.Users.ToList()
                : db.Users.Where(u => u.Roles.Any(ur => ur.RoleId == roleProf.Id)).ToList();

            ViewBag.UserId = new SelectList(profesores.OrderBy(u => u.Email), "Id", "Email", userId);
            ViewBag.SpecialtyId = new SelectList(db.Specialties.OrderBy(s => s.Name), "Id", "Name", specialtyId);

            var termItems = db.Terms
                .OrderBy(t => t.Year).ThenBy(t => t.OrderInYear)
                .Select(t => new { t.Id, Label = t.Year + " - " + t.Label })
                .ToList();
            ViewBag.TermId = new SelectList(termItems, "Id", "Label", termId);
        }

        // ============ DISPOSE ============
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }

    // ViewModel para /Access/List
    public class AccessListViewModel
    {
        public int Id { get; set; }
        public string Profesor { get; set; }
        public string Especialidad { get; set; }
        public string Periodo { get; set; }
        public DateTime FechaAcceso { get; set; }
        public DateTime? FechaLimite { get; set; }

        // Acciones por fila
        public int SpecialtyId { get; set; }
        public int TermId { get; set; }

        // NUEVO: precio por acceso (null/0 = gratis)
        public decimal? PriceCRC { get; set; }
    }
}
