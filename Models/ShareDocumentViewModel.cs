using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace GestionDocumentosMVC.Models
{
    public class ShareDocumentViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un usuario.")]
        [Display(Name = "Compartir con el usuario")]
        public int SelectedUserId { get; set; }

        [Required(ErrorMessage = "Debes seleccionar el nivel de permiso.")]
        [Display(Name = "Nivel de Permiso")]
        public int PermissionId { get; set; }

        public IEnumerable<SelectListItem> UsersList { get; set; }
        public IEnumerable<SelectListItem> PermissionsList { get; set; }
    }
}