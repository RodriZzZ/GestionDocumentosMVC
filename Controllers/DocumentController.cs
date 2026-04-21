using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using GestionDocumentosMVC.Models;
using GestionDocumentosMVC.Data;

namespace GestionDocumentosMVC.Controllers
{
    public class DocumentController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var listaDocumentos = new List<DocumentViewModel>();

            int currentUserId = Convert.ToInt32(Session["UserId"]);

            using (var conn = Database.GetConnection())
            {
                using (var cmd = new SqlCommand("sp_GetDocumentsByUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserId", currentUserId);
                    cmd.Parameters.AddWithValue("@SortOption", 0);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listaDocumentos.Add(new DocumentViewModel
                            {
                                Id = Convert.ToInt32(reader["DocumentId"]),
                                Name = reader["DocumentName"].ToString(),
                                Extension = reader["FileExtension"].ToString(),
                                UploadDate = Convert.ToDateTime(reader["UploadDate"]),
                                Version = Convert.ToInt32(reader["VersionNumber"])
                            });
                        }
                    }
                }
            }

            return View(listaDocumentos);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(DocumentUploadModel model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                if (model.File != null && model.File.ContentLength > 0)
                {
                    try
                    {
                        int currentUserId = Convert.ToInt32(Session["UserId"]);

                        byte[] fileBytes;
                        using (var binaryReader = new BinaryReader(model.File.InputStream))
                        {
                            fileBytes = binaryReader.ReadBytes(model.File.ContentLength);
                        }

                        string fileName = Path.GetFileNameWithoutExtension(model.File.FileName);
                        string fileExtension = Path.GetExtension(model.File.FileName);

                        using (var conn = Database.GetConnection())
                        {
                            using (var cmd = new SqlCommand("sp_UploadNewDocument", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.AddWithValue("@Name", fileName);
                                cmd.Parameters.AddWithValue("@FileExtension", fileExtension);

                                cmd.Parameters.AddWithValue("@OwnerUserId", currentUserId);

                                cmd.Parameters.AddWithValue("@FileContent", fileBytes);
                                cmd.Parameters.AddWithValue("@FileSizeInBytes", model.File.ContentLength);

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }
                        }

                        TempData["MensajeExito"] = "Documento subido correctamente.";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error de base de datos: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("File", "Por favor, selecciona un archivo válido.");
                }
            }
            return View(model);
        }
        public ActionResult Download(int id)
        {
            try
            {
                int currentUserId = Convert.ToInt32(Session["UserId"]);
                byte[] fileBytes = null;
                string fullName = "";

                using (var conn = Database.GetConnection())
                {
                    string query = @"
                        SELECT TOP 1 d.name, d.file_extension, v.file_content 
                        FROM Documents d
                        INNER JOIN DocumentVersion v ON d.id = v.document_id
                        WHERE d.id = @id AND d.owner_user_id = @userId
                        ORDER BY v.version_number DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@userId", currentUserId);

                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                fullName = reader["name"].ToString() + reader["file_extension"].ToString();
                                fileBytes = (byte[])reader["file_content"];
                            }
                        }
                    }
                }

                if (fileBytes != null)
                {
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fullName);
                }
                else
                {
                    TempData["MensajeError"] = "El documento no existe o no tienes permisos para descargarlo.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al intentar descargar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult UpdateVersion(int id, string name)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Auth");

            var model = new UpdateDocumentViewModel
            {
                DocumentId = id,
                DocumentName = name
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateVersion(UpdateDocumentViewModel model)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid && model.File != null && model.File.ContentLength > 0)
            {
                try
                {
                    int currentUserId = Convert.ToInt32(Session["UserId"]);
                    byte[] fileBytes;
                    using (var binaryReader = new BinaryReader(model.File.InputStream))
                    {
                        fileBytes = binaryReader.ReadBytes(model.File.ContentLength);
                    }

                    string fileExtension = Path.GetExtension(model.File.FileName);

                    using (var conn = Database.GetConnection())
                    {
                        using (var cmd = new SqlCommand("sp_AddNewDocumentVersion", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@DocumentId", model.DocumentId);
                            cmd.Parameters.AddWithValue("@UploadingUserId", currentUserId);
                            cmd.Parameters.AddWithValue("@FileContent", fileBytes);
                            cmd.Parameters.AddWithValue("@FileSizeInBytes", model.File.ContentLength);
                            cmd.Parameters.AddWithValue("@FileExtension", fileExtension);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    TempData["MensajeExito"] = "¡Nueva versión del documento subida correctamente!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                }
            }
            return View(model);
        }
        public ActionResult Delete(int id)
        {
            try
            {
                int currentUserId = Convert.ToInt32(Session["UserId"]);

                using (var conn = Database.GetConnection())
                {
                    using (var cmd = new SqlCommand("sp_DeleteDocument", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DocumentId", id);
                        cmd.Parameters.AddWithValue("@UserId", currentUserId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                TempData["MensajeExito"] = "Documento eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Share(int id, string name)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Auth");
            int currentUserId = Convert.ToInt32(Session["UserId"]);

            var model = new ShareDocumentViewModel { DocumentId = id, DocumentName = name };

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var users = new List<SelectListItem>();
                using (var cmd = new SqlCommand("SELECT id, institutional_email FROM Users WHERE id != @CurrentId", conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentId", currentUserId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new SelectListItem { Value = reader["id"].ToString(), Text = reader["institutional_email"].ToString() });
                        }
                    }
                }
                model.UsersList = users;

                var permissions = new List<SelectListItem>();
                using (var cmd = new SqlCommand("SELECT id, name FROM DocumentAccess WHERE id IN (1, 2)", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissions.Add(new SelectListItem { Value = reader["id"].ToString(), Text = reader["name"].ToString() });
                        }
                    }
                }
                model.PermissionsList = permissions;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Share(ShareDocumentViewModel model)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Auth");
            int currentUserId = Convert.ToInt32(Session["UserId"]);

            try
            {
                using (var conn = Database.GetConnection())
                {
                    string query = @"
                        IF EXISTS (SELECT 1 FROM UserDocumentAccess WHERE document_id = @DocId AND access_granted_to_user_id = @ToUser)
                        BEGIN
                            UPDATE UserDocumentAccess SET permission_id = @PermId, access_granted_by_user_id = @ById 
                            WHERE document_id = @DocId AND access_granted_to_user_id = @ToUser
                        END
                        ELSE
                        BEGIN
                            INSERT INTO UserDocumentAccess (document_id, access_granted_to_user_id, permission_id, access_granted_by_user_id)
                            VALUES (@DocId, @ToUser, @PermId, @ById)
                        END";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DocId", model.DocumentId);
                        cmd.Parameters.AddWithValue("@ToUser", model.SelectedUserId);
                        cmd.Parameters.AddWithValue("@PermId", model.PermissionId);
                        cmd.Parameters.AddWithValue("@ById", currentUserId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                TempData["MensajeExito"] = "Permisos actualizados correctamente. El usuario ahora tiene acceso.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al compartir el documento: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
        public ActionResult SharedWithMe()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Auth");
            int currentUserId = Convert.ToInt32(Session["UserId"]);

            var lista = new List<SharedDocumentViewModel>();

            using (var conn = Database.GetConnection())
            {
                string query = @"
                    SELECT 
                        d.id AS DocumentId, 
                        d.name AS DocumentName, 
                        d.file_extension AS FileExtension, 
                        u.first_name + ' ' + u.last_name AS OwnerName,
                        da.name AS PermissionName,
                        (SELECT MAX(version_number) FROM DocumentVersion WHERE document_id = d.id) AS Version
                    FROM UserDocumentAccess uda
                    INNER JOIN Documents d ON uda.document_id = d.id
                    INNER JOIN Users u ON d.owner_user_id = u.id
                    INNER JOIN DocumentAccess da ON uda.permission_id = da.id
                    WHERE uda.access_granted_to_user_id = @UserId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", currentUserId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new SharedDocumentViewModel
                            {
                                Id = Convert.ToInt32(reader["DocumentId"]),
                                Name = reader["DocumentName"].ToString(),
                                Extension = reader["FileExtension"].ToString(),
                                OwnerName = reader["OwnerName"].ToString(),
                                Permission = reader["PermissionName"].ToString(),
                                Version = reader["Version"] != DBNull.Value ? Convert.ToInt32(reader["Version"]) : 1
                            });
                        }
                    }
                }
            }
            return View(lista);
        }

        public ActionResult DownloadShared(int id)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Auth");
            int currentUserId = Convert.ToInt32(Session["UserId"]);

            byte[] fileBytes = null;
            string fullName = "";

            try
            {
                using (var conn = Database.GetConnection())
                {
                    string query = @"
                        SELECT TOP 1 d.name, d.file_extension, v.file_content 
                        FROM Documents d
                        INNER JOIN DocumentVersion v ON d.id = v.document_id
                        INNER JOIN UserDocumentAccess uda ON d.id = uda.document_id
                        WHERE d.id = @id AND uda.access_granted_to_user_id = @userId
                        ORDER BY v.version_number DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@userId", currentUserId);

                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                fullName = reader["name"].ToString() + reader["file_extension"].ToString();
                                fileBytes = (byte[])reader["file_content"];
                            }
                        }
                    }
                }

                if (fileBytes != null)
                {
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fullName);
                }

                TempData["MensajeError"] = "No tienes permiso para descargar este documento.";
                return RedirectToAction("SharedWithMe");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al intentar descargar: " + ex.Message;
                return RedirectToAction("SharedWithMe");
            }
        }
    }
}