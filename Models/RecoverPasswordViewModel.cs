using System.ComponentModel.DataAnnotations;

namespace GestionDocumentosMVC.Models
{
    public class RecoverPasswordViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [Display(Name = "Correo Institucional")]
        public string Email { get; set; }
    }
}