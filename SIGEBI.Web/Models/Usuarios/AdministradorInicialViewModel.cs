using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Web.Models.Usuarios;

public class AdministradorInicialViewModel
{
    [Required(ErrorMessage = "Escribe el identificador.")]
    [StringLength(30, ErrorMessage = "El identificador no puede tener más de 30 caracteres.")]
    [Display(Name = "Identificador (matrícula o código)")]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el nombre.")]
    [StringLength(80, ErrorMessage = "El nombre no puede tener más de 80 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el apellido.")]
    [StringLength(80, ErrorMessage = "El apellido no puede tener más de 80 caracteres.")]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el correo.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe una contraseña.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Repite la contraseña")]
    public string? ConfirmarContrasena { get; set; }
}