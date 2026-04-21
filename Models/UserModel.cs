using System;
using System.ComponentModel.DataAnnotations;

namespace GestionDocumentosMVC.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        [Display(Name = "Nombres")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres.")]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un formato de correo válido.")]
        [Display(Name = "Correo Institucional")]
        public string InstitutionalEmail { get; set; }

        // La contraseña solo será obligatoria al crear
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un rol.")]
        [Display(Name = "Rol del Usuario")]
        public int RoleId { get; set; }
    }
}