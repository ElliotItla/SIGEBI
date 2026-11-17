using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Usuarios;

public class CrearUsuarioRequest
{
    /// <summary>Matrícula o código único del usuario.</summary>
    /// <example>2025-0100</example>
    [Required(ErrorMessage = "Escribe el identificador.")]
    [StringLength(30, ErrorMessage = "El identificador no puede tener más de 30 caracteres.")]
    public string Identificador { get; set; } = string.Empty;

    /// <example>Sofía</example>
    [Required(ErrorMessage = "Escribe el nombre.")]
    [StringLength(80, ErrorMessage = "El nombre no puede tener más de 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <example>Reyes</example>
    [Required(ErrorMessage = "Escribe el apellido.")]
    [StringLength(80, ErrorMessage = "El apellido no puede tener más de 80 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    /// <example>sofia@itla.edu.do</example>
    [Required(ErrorMessage = "Escribe el correo.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Estudiante, Docente, PersonalBibliotecario, Administrador o Auditor.</summary>
    /// <example>Estudiante</example>
    [Required(ErrorMessage = "Elige el tipo de usuario.")]
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Al menos 8 caracteres.</summary>
    /// <example>ClaveSegura1</example>
    [Required(ErrorMessage = "Escribe una contraseña.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;
}