using System.Web.Mvc;

namespace GestionDocumentosMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Si nadie ha iniciado sesión, lo mandamos al Login
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }
    }
}