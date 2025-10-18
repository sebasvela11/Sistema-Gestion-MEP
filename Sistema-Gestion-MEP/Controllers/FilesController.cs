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
            // Obtener accesos existentes para seleccionar
            var accesos = _db.SpecialtyAccesses
                .Select(a => new
                {
                    a.Id,
                    Label = a.Specialty.Name + " - " + a.Term.Label + " " + a.Term.Year + " - " + a.User.Email
                })
                .OrderBy(a => a.Label)
                .ToList();

            ViewBag.SpecialtyAccessId = new SelectList(accesos, "Id", "Label");

            return View();
        }

        [HttpPost, Authorize(Roles = "Coordinador"), ValidateAntiForgeryToken]
        public ActionResult UploadTemplate(int SpecialtyAccessId, decimal? PriceCRC, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                TempData["err"] = "Debe seleccionar un archivo PDF.";
                return RedirectToAction("UploadTemplate");
            }

            if (!IsPdf(file))
            {
                TempData["err"] = "Solo se permiten archivos PDF.";
                return RedirectToAction("UploadTemplate");
            }

            // Validar precio
            if (PriceCRC.HasValue && PriceCRC.Value < 0)
            {
                TempData["err"] = "El precio no puede ser negativo.";
                return RedirectToAction("UploadTemplate");
            }

            // Verificar que el acceso existe
            var access = _db.SpecialtyAccesses.Find(SpecialtyAccessId);
            if (access == null)
            {
                TempData["err"] = "El acceso seleccionado no existe.";
                return RedirectToAction("UploadTemplate");
            }

            // Asegurar directorio
            EnsureDir(TEMPLATES_DIR);

            // Nombre unico para evitar colisiones
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
                SpecialtyAccessId = SpecialtyAccessId,
                FileName = originalName,
                StoredPath = relPath,
                FileSizeBytes = file.ContentLength,
                UploadedAtUtc = DateTime.UtcNow,
                PriceCRC = PriceCRC // null = gratis, > 0 = de pago
            };

            _db.Documents.Add(doc);
            _db.SaveChanges();

            var priceText = PriceCRC.HasValue && PriceCRC.Value > 0
                ? $" (Precio: ₡{PriceCRC.Value:N0})"
                : " (Gratis)";
            LogActivity("Upload", "Document", doc.Id.ToString(), $"Plantilla {originalName}{priceText}", success: true);

            TempData["ok"] = PriceCRC.HasValue && PriceCRC.Value > 0
                ? $"Plantilla subida correctamente. Precio: ₡{PriceCRC.Value:N0}"
                : "Plantilla subida correctamente. Documento gratuito.";

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
                    // Verificar que el documento pertenece a un acceso del usuario
                    var access = _db.SpecialtyAccesses.FirstOrDefault(a =>
                        a.Id == doc.SpecialtyAccessId &&
                        a.UserId == userId);

                    if (access == null)
                    {
                        TempData["err"] = "No tiene acceso a esta plantilla.";
                        LogActivity("Download", "Document", doc.Id.ToString(), "Sin acceso", success: false);
                        return RedirectToAction("Index", "Profesor");
                    }

                    // REGLA DE PRECIO (DESDE DOCUMENTO):
                    // - Gratis si doc.PriceCRC es null o <= 0
                    // - Si doc.PriceCRC > 0, requiere PaymentSimulation con DocumentId en estado "Paid"
                    var price = doc.PriceCRC ?? 0m;
                    if (price > 0m)
                    {
                        var payment = _db.PaymentSimulations.FirstOrDefault(p =>
                            p.UserId == userId &&
                            p.DocumentId == doc.Id &&
                            p.PaymentType == "Document");

                        var paid = payment != null &&
                                   string.Equals(payment.Status, "Paid", StringComparison.OrdinalIgnoreCase);

                        if (!paid)
                        {
                            TempData["err"] = $"Debe simular el pago (₡{price:N0}) antes de descargar este documento.";
                            // Redirigir a simulacion de pago de documento
                            return RedirectToAction("SimulateDocument", "Payments", new { documentId = doc.Id });
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
                TempData["err"] = "El archivo fisico no existe en el servidor.";
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
                    a.Id,
                    Label = a.Specialty.Name + " - " + a.Term.Label + " " + a.Term.Year,
                    a.DeadlineUtc
                })
                .ToList();

            ViewBag.SpecialtyAccessId = new SelectList(myAccess, "Id", "Label");

            return View();
        }

        [HttpPost, Authorize(Roles = "Profesor"), ValidateAntiForgeryToken]
        public ActionResult UploadSubmission(int SpecialtyAccessId, HttpPostedFileBase file)
        {
            var userId = User.Identity.GetUserId();

            if (file == null || file.ContentLength == 0 || !IsPdf(file))
            {
                TempData["err"] = "Debe seleccionar un archivo PDF valido.";
                return RedirectToAction("UploadSubmission");
            }

            var access = _db.SpecialtyAccesses.FirstOrDefault(a =>
                a.Id == SpecialtyAccessId && a.UserId == userId);

            if (access == null)
            {
                TempData["err"] = "No tiene acceso a esa especialidad/periodo.";
                return RedirectToAction("UploadSubmission");
            }

            // Validar fecha limite (si existe)
            if (access.DeadlineUtc.HasValue && DateTime.UtcNow > access.DeadlineUtc.Value)
            {
                TempData["err"] = "La fecha limite ha vencido. No se puede subir la entrega.";
                LogActivity("Upload", "Document", null, "Entrega fuera de fecha", success: false);
                return RedirectToAction("UploadSubmission");
            }

            EnsureDir(SUBMISSIONS_DIR);

            var originalName = Path.GetFileName(file.FileName);
            var safeName = MakeSafeFileName(originalName);
            var uniqueName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeName}";

            // Guardamos por usuario para organizacion
            var userFolder = $"{VirtualPathUtility.ToAbsolute(SUBMISSIONS_DIR).TrimEnd('/')}/{userId}";
            EnsureDir(userFolder);
            var relPath = $"{userFolder}/{uniqueName}";
            var absPath = Server.MapPath(relPath);
            file.SaveAs(absPath);

            var doc = new Document
            {
                Type = 2, // Entrega
                OwnerUserId = userId,
                SpecialtyAccessId = SpecialtyAccessId,
                FileName = originalName,
                StoredPath = relPath,
                FileSizeBytes = file.ContentLength,
                UploadedAtUtc = DateTime.UtcNow,
                PriceCRC = null // Las entregas no tienen precio
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