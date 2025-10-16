using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Sistema_Gestion_MEP.Models;

namespace Sistema_Gestion_MEP.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // Descarga segura
        [HttpGet]
        public ActionResult Download(int id)
        {
            var doc = _db.Documents.Find(id);
            if (doc == null) return HttpNotFound();

            var userId = User.Identity.GetUserId();

            // Si es plantilla (Type=1), validar acceso
            if (doc.Type == 1)
            {
                bool hasAccess = _db.SpecialtyAccesses
                    .Any(a => a.UserId == userId && a.SpecialtyId == doc.SpecialtyId && a.TermId == doc.TermId);
                if (!hasAccess) return new HttpUnauthorizedResult();

                // (Opcional) exigir Paid:
                // var pay = _db.PaymentSimulations.FirstOrDefault(p => p.UserId == userId && p.SpecialtyId == doc.SpecialtyId && p.TermId == doc.TermId);
                // if (pay == null || pay.Status != "Paid") return new HttpStatusCodeResult(402, "Pago simulado requerido");
            }
            else
            {
                // Si es entrega (Type=2), solo dueño o coordinador
                if (doc.OwnerUserId != userId && !User.IsInRole("Coordinador"))
                    return new HttpUnauthorizedResult();
            }

            var absPath = Server.MapPath(doc.StoredPath);
            if (!System.IO.File.Exists(absPath)) return HttpNotFound();

            // Log
            _db.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                Action = "Download",
                Entity = "Document",
                EntityId = doc.Id.ToString(),
                Info = doc.FileName,
                Ip = Request.UserHostAddress,
                UserAgent = Request.UserAgent,
                CreatedAtUtc = DateTime.UtcNow
            });
            _db.SaveChanges();

            return File(absPath, "application/pdf", doc.FileName);
        }

        // Subida de entrega (Type=2)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UploadSubmission(int specialtyId, int termId, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return new HttpStatusCodeResult(400, "Archivo requerido");

            var userId = User.Identity.GetUserId();

            // verificar acceso
            bool hasAccess = _db.SpecialtyAccesses.Any(a => a.UserId == userId && a.SpecialtyId == specialtyId && a.TermId == termId);
            if (!hasAccess) return new HttpUnauthorizedResult();

            // guardar en App_Data/uploads
            var safeName = Path.GetFileName(file.FileName);
            var relPath = $"~/App_Data/uploads/{Guid.NewGuid()}_{safeName}";
            var absPath = Server.MapPath(relPath);
            file.SaveAs(absPath);

            using (var trx = _db.Database.BeginTransaction())
            {
                try
                {
                    var doc = new Document
                    {
                        Type = 2,
                        OwnerUserId = userId,
                        SpecialtyId = specialtyId,
                        TermId = termId,
                        FileName = safeName,
                        StoredPath = relPath,
                        UploadedAtUtc = DateTime.UtcNow
                    };
                    _db.Documents.Add(doc);
                    _db.ActivityLogs.Add(new ActivityLog
                    {
                        UserId = userId,
                        Action = "Upload",
                        Entity = "Document",
                        EntityId = "(new)",
                        Info = safeName,
                        Ip = Request.UserHostAddress,
                        UserAgent = Request.UserAgent,
                        CreatedAtUtc = DateTime.UtcNow
                    });
                    _db.SaveChanges();
                    trx.Commit();
                }
                catch
                {
                    trx.Rollback();
                    if (System.IO.File.Exists(absPath)) System.IO.File.Delete(absPath);
                    throw;
                }
            }

            return RedirectToAction("Index", "Profesor");
        }
    }
}
