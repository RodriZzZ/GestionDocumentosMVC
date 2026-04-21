using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using GestionDocumentosMVC.Models;
using GestionDocumentosMVC.Data;

namespace GestionDocumentosMVC.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var conn = Database.GetConnection())
                {
                    string query = "SELECT id, first_name, password, role_id FROM Users WHERE institutional_email = @Email";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hashDb = reader["password"].ToString();

                                if (HashPassword.Verify(model.Password, hashDb))
                                {
                                    Session["UserId"] = Convert.ToInt32(reader["id"]);
                                    Session["UserName"] = reader["first_name"].ToString();
                                    Session["UserRole"] = Convert.ToInt32(reader["role_id"]);

                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }
                    }
                }
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
        [HttpGet]
        public ActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var conn = Database.GetConnection())
                {
                    string checkQuery = "SELECT id FROM Users WHERE institutional_email = @Email";
                    int? userId = null;

                    using (var checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", model.Email);
                        conn.Open();
                        var result = checkCmd.ExecuteScalar();
                        if (result != null)
                        {
                            userId = Convert.ToInt32(result);
                        }
                    }

                    if (userId.HasValue)
                    {
                        string tempPassword = "Temp" + new Random().Next(1000, 9999) + "!";

                        string hashedPassword = HashPassword.Hash(tempPassword);

                        string updateQuery = "UPDATE Users SET password = @Password WHERE id = @Id";
                        using (var updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                            updateCmd.Parameters.AddWithValue("@Id", userId.Value);
                            updateCmd.ExecuteNonQuery();
                        }

                        TempData["MensajeExito"] = $"Tu contraseña temporal ha sido generada: [{tempPassword}] -> Cópiala SIN los corchetes ni espacios.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "No se encontró ningún usuario registrado con ese correo.");
                    }
                }
            }
            return View(model);
        }
    }
}