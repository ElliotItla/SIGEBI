using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Web.Models.Usuarios;

public class IngresarViewModel
{
    [Required(ErrorMessage = "Escribe tu correo.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    // Página a la que volver después de entrar
    public string? ReturnUrl { get; set; }
}