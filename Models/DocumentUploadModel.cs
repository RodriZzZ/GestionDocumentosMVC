using System.ComponentModel.DataAnnotations;
using System.Web; //para HttpPostedFileBase

namespace GestionDocumentosMVC.Models
{
    public class DocumentUploadModel
    {
        [Required(ErrorMessage = "Por favor, selecciona un archivo.")]
        [Display(Name = "Archivo a subir")]
        public HttpPostedFileBase File { get; set; }
        // HttpPostedFileBase es la versión MVC del viejo control FileUpload
    }
}