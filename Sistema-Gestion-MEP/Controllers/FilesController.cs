using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Sistema_Gestion_MEP.Models;

namespace Sistema_Gestion_MEP.Controllers
{
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // Carpetas base (App_Data no es servida directamente por IIS: seguro para almacenar)
        private const string TEMPLATES_DIR = "~/App_Data/templates";
        private const string SUBMISSIONS_DIR = "~/App_Data/submissions";

        // =======================
        // 1) SUBIR PLANTILLAS (COORDINADOR)
        // =======================
        [Authorize(Roles = "Coordinador")]
        public ActionResult UploadTemplate(int? specialtyId, int? termId)
        {
            ViewBag.SpecialtyId = new SelectList(
                _db.Specialties.OrderBy(s => s.Name), "Id", "Name", specialtyId
            );

            // Mostramos "2025 - I Trimestre"
            var termItems = _db.Terms
                .OrderBy(t => t.Year).ThenBy(t => t.OrderInYear)
                .Select(t => new { t.Id, Label = t.Year + " - " + t.Label })
                .ToList();
            ViewBag.TermId = new SelectList(termItems, "Id", "Label", termId);

            return View();
        }

        [HttpPost, Authorize(Roles = "Coordinador"), ValidateAntiForgeryToken]
        public ActionResult UploadTemplate(int SpecialtyId, int TermId, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                TempData["err"] = "Debe seleccionar un archivo PDF.";
                return RedirectToAction("UploadTemplate", new { specialtyId = SpecialtyId, termId = TermId });
            }

            if (!IsPdf(file))
            {
                TempData["err"] = "Solo se permiten archivos PDF.";
                return RedirectToAction("UploadTemplate", new { specialtyId = SpecialtyId, termId = TermId });
            }

            // Asegurar directorio
            EnsureDir(TEMPLATES_DIR);

            // Nombre único para evitar colisiones
            var originalName = Path.GetFileName(file.FileName);
            var safeName = MakeSafeFileName(originalName);
            var uniqueName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeName}";

            var relPath = $"{VirtualPathUtility.ToAbsolute(TEMPLATES_DIR).TrimEnd('/')}/{uniqueName}";
            var absPath = Server.MapPath(relPath);
            file.SaveAs(absPath);

            var doc = new Document
            {
                Type = 1, // Plantilla
                OwnerUserId = null, // plantillas no tienen owner
                SpecialtyId = SpecialtyId,
                TermId = TermId,
                FileName = originalName,
                StoredPath = relPath,           // guardamos ruta virtual relativa
                FileSizeBytes = file.ContentLength,
                UploadedAtUtc = DateTime.UtcNow
            };

            _db.Documents.Add(doc);
            _db.SaveChanges();

            LogActivity("Upload", "Document", doc.Id.ToString(), $"Plantilla {originalName} subida", success: true);

            TempData["ok"] = "Plantilla subida correctamente.";
            return RedirectToAction("List", "Access");
        }

        // =======================
        // 2) DESCARGAR (PLANTILLAS / ENTREGAS)
        // =======================
        [Authorize]
        public ActionResult Download(int id)
        {
            var userId = User.Identity.GetUserId();
            var doc = _db.Documents.FirstOrDefault(d => d.Id == id);
            if (doc == null)
            {
                TempData["err"] = "Documento no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            if (doc.Type == 1) // Plantilla
            {
                if (!User.IsInRole("Coordinador"))
                {
                    // Debe tener acceso a esa especialidad/periodo
                    var access = _db.SpecialtyAccesses.FirstOrDefault(a =>
                        a.UserId == userId &&
                        a.SpecialtyId == doc.SpecialtyId &&
                        a.TermId == doc.TermId);

                    if (access == null)
                    {
                        TempData["err"] = "No tiene acceso a esta plantilla.";
                        LogActivity("Download", "Document", doc.Id.ToString(), "Sin acceso", success: false);
                        return RedirectToAction("Index", "Profesor");
                    }

                    // REGLA DE PRECIO:
                    // - Gratis si PriceCRC es null o <= 0
                    // - Si PriceCRC > 0, requiere PaymentSimulation en estado "Paid"
                    var price = access.PriceCRC ?? 0m;
                    if (price > 0m)
                    {
                        var payment = _db.PaymentSimulations.FirstOrDefault(p =>
                            p.UserId == userId &&
                            p.SpecialtyId == doc.SpecialtyId &&
                            p.TermId == doc.TermId);

                        var paid = payment != null &&
                                   string.Equals(payment.Status, "Paid", StringComparison.OrdinalIgnoreCase);

                        if (!paid)
                        {
                            TempData["err"] = $"Debe simular el pago (₡{price:N0}) antes de descargar la plantilla.";
                            // El PaymentsController tomará el monto desde SpecialtyAccess.PriceCRC
                            return RedirectToAction("Simulate", "Payments", new { specialtyId = doc.SpecialtyId, termId = doc.TermId });
                        }
                    }
                }
            }
            else if (doc.Type == 2) // Entrega
            {
                // Propietario o Coordinador
                if (!User.IsInRole("Coordinador"))
                {
                    if (!string.Equals(doc.OwnerUserId, userId, StringComparison.Ordinal))
                    {
                        TempData["err"] = "No tiene permisos para descargar esta entrega.";
                        LogActivity("Download", "Document", doc.Id.ToString(), "Entrega sin permiso", success: false);
                        return RedirectToAction("Index", "Profesor");
                    }
                }
            }

            // Descargar
            var physical = MapPathSafe(doc.StoredPath);
            if (!System.IO.File.Exists(physical))
            {
                TempData["err"] = "El archivo físico no existe en el servidor.";
                LogActivity("Download", "Document", doc.Id.ToString(), "Archivo no encontrado", success: false);
                return RedirectToAction("Index", "Home");
            }

            LogActivity("Download", "Document", doc.Id.ToString(), $"Descarga {doc.FileName}", success: true);
            return File(physical, "application/pdf", doc.FileName);
        }

        // =======================
        // 3) SUBIR ENTREGA (PROFESOR)
        // =======================
        [Authorize(Roles = "Profesor")]
        public ActionResult UploadSubmission()
        {
            var userId = User.Identity.GetUserId();

            // Accesos del profesor para poblar combos
            var myAccess = _db.SpecialtyAccesses
                .Where(a => a.UserId == userId)
                .Select(a => new
                {
                    a.SpecialtyId,
                    a.TermId,
                    SpecialtyName = a.Specialty.Name,
                    TermLabel = a.Term.Label,
                    a.DeadlineUtc
                })
                .ToList();

            // Selects agrupados por texto "Nombre — Label"
            ViewBag.AccessKey = new SelectList(
                myAccess.Select(a => new
                {
                    Key = $"{a.SpecialtyId}|{a.TermId}",
                    Text = $"{a.SpecialtyName} — {a.TermLabel}"
                }),
                "Key", "Text");

            return View();
        }

        [HttpPost, Authorize(Roles = "Profesor"), ValidateAntiForgeryToken]
        public ActionResult UploadSubmission(string AccessKey, HttpPostedFileBase file)
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrWhiteSpace(AccessKey) || !AccessKey.Contains("|"))
            {
                TempData["err"] = "Debe seleccionar una especialidad y periodo.";
                return RedirectToAction("UploadSubmission");
            }

            if (file == null || file.ContentLength == 0 || !IsPdf(file))
            {
                TempData["err"] = "Debe seleccionar un archivo PDF válido.";
                return RedirectToAction("UploadSubmission");
            }

            var parts = AccessKey.Split('|');
            int specialtyId = int.Parse(parts[0]);
            int termId = int.Parse(parts[1]);

            var access = _db.SpecialtyAccesses.FirstOrDefault(a =>
                a.UserId == userId && a.SpecialtyId == specialtyId && a.TermId == termId);

            if (access == null)
            {
                TempData["err"] = "No tiene acceso a esa especialidad/periodo.";
                return RedirectToAction("UploadSubmission");
            }

            // Validar fecha límite (si existe)
            if (access.DeadlineUtc.HasValue && DateTime.UtcNow > access.DeadlineUtc.Value)
            {
                TempData["err"] = "La fecha límite ha vencido. No se puede subir la entrega.";
                LogActivity("Upload", "Document", null, "Entrega fuera de fecha", success: false);
                return RedirectToAction("UploadSubmission");
            }

            EnsureDir(SUBMISSIONS_DIR);

            var originalName = Path.GetFileName(file.FileName);
            var safeName = MakeSafeFileName(originalName);
            var uniqueName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeName}";

            // Guardamos por usuario para organización
            var userFolder = $"{VirtualPathUtility.ToAbsolute(SUBMISSIONS_DIR).TrimEnd('/')}/{userId}";
            EnsureDir(userFolder);
            var relPath = $"{userFolder}/{uniqueName}";
            var absPath = Server.MapPath(relPath);
            file.SaveAs(absPath);

            var doc = new Document
            {
                Type = 2, // Entrega
                OwnerUserId = userId,
                SpecialtyId = specialtyId,
                TermId = termId,
                FileName = originalName,
                StoredPath = relPath,
                FileSizeBytes = file.ContentLength,
                UploadedAtUtc = DateTime.UtcNow
            };

            _db.Documents.Add(doc);
            _db.SaveChanges();

            LogActivity("Upload", "Document", doc.Id.ToString(), $"Entrega {originalName}", success: true);

            TempData["ok"] = "Entrega subida correctamente.";
            return RedirectToAction("Index", "Profesor");
        }

        // =======================
        // Helpers
        // =======================
        private bool IsPdf(HttpPostedFileBase file)
        {
            if (file == null) return false;
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return ext == ".pdf";
        }

        private void EnsureDir(string virtualPath)
        {
            var abs = MapPathSafe(virtualPath);
            if (!Directory.Exists(abs))
                Directory.CreateDirectory(abs);
        }

        private string MakeSafeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        private string MapPathSafe(string storedPath)
        {
            // Acepta "~/" o rutas relativas tipo "/App_Data/..."
            var path = storedPath.StartsWith("~")
                ? storedPath
                : (storedPath.StartsWith("/") ? "~" + storedPath : "~/" + storedPath);
            return Server.MapPath(path);
        }

        private void LogActivity(string action, string entity, string entityId, string info, bool success)
        {
            try
            {
                var log = new ActivityLog
                {
                    UserId = User?.Identity?.GetUserId(),
                    Action = action,
                    Entity = entity,
                    EntityId = entityId,
                    Info = info,
                    Ip = Request?.UserHostAddress,
                    UserAgent = Request?.UserAgent,
                    Success = success,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _db.ActivityLogs.Add(log);
                _db.SaveChanges();
            }
            catch
            {
                // avoided: errores de logging no deben romper el flujo
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
