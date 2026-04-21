using System.ComponentModel.DataAnnotations;
using System.Web;

namespace GestionDocumentosMVC.Models
{
    public class UpdateDocumentViewModel
    {
        public int DocumentId { get; set; }

        public string DocumentName { get; set; }

        [Required(ErrorMessage = "Por favor, selecciona el nuevo archivo.")]
        [Display(Name = "Nuevo Archivo (Siguiente Versión)")]
        public HttpPostedFileBase File { get; set; }
    }
}