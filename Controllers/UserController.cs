using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using GestionDocumentosMVC.Models; 
using GestionDocumentosMVC.Data;

namespace GestionDocumentosMVC.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            var listaUsuarios = new List<UserModel>();

            try
            {
                using (var conn = Database.GetConnection())
                {
                    string query = "SELECT id, first_name, last_name, institutional_email, role_id FROM Users";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listaUsuarios.Add(new UserModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    FirstName = reader["first_name"].ToString(),
                                    LastName = reader["last_name"].ToString(),
                                    InstitutionalEmail = reader["institutional_email"].ToString(),
                                    RoleId = Convert.ToInt32(reader["role_id"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al cargar los datos: " + ex.Message;
            }

            return View(listaUsuarios);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(UserModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var conn = Database.GetConnection())
                    {
                        using (var cmd = new SqlCommand("sp_CreateUser", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                            cmd.Parameters.AddWithValue("@LastName", model.LastName);
                            cmd.Parameters.AddWithValue("@Email", model.InstitutionalEmail);

                            cmd.Parameters.AddWithValue("@PasswordHash", HashPassword.Hash(model.Password));

                            cmd.Parameters.AddWithValue("@RoleId", model.RoleId);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error en la base de datos: " + ex.Message);
                }
            }
            return View(model);
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            UserModel user = new UserModel();
            using (var conn = Database.GetConnection())
            {
                string query = "SELECT id, first_name, last_name, institutional_email, role_id FROM Users WHERE id = @id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user.Id = Convert.ToInt32(reader["id"]);
                            user.FirstName = reader["first_name"].ToString();
                            user.LastName = reader["last_name"].ToString();
                            user.InstitutionalEmail = reader["institutional_email"].ToString();
                            user.RoleId = Convert.ToInt32(reader["role_id"]);
                        }
                    }
                }
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Edit(UserModel model)
        {
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                using (var conn = Database.GetConnection())
                {
                    string query = "UPDATE Users SET first_name = @FirstName, last_name = @LastName, institutional_email = @Email, role_id = @RoleId WHERE id = @Id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", model.LastName);
                        cmd.Parameters.AddWithValue("@Email", model.InstitutionalEmail);
                        cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                        cmd.Parameters.AddWithValue("@Id", model.Id);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    string query = "DELETE FROM Users WHERE id = @id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                TempData["MensajeExito"] = "Usuario eliminado correctamente.";
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    TempData["MensajeError"] = "No se puede eliminar el usuario porque tiene documentos subidos en el sistema.";
                }
                else
                {
                    TempData["MensajeError"] = "Error de base de datos: " + ex.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Ocurrió un error inesperado: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}