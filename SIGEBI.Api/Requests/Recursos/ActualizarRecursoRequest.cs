using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Recursos;

// El código del recurso no se puede cambiar
public class ActualizarRecursoRequest
{
    /// <example>Clean Architecture (2.ª edición)</example>
    [Required(ErrorMessage = "Escribe el título.")]
    [StringLength(200, ErrorMessage = "El título no puede tener más de 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <example>Robert C. Martin</example>
    [Required(ErrorMessage = "Escribe el autor.")]
    [StringLength(150, ErrorMessage = "El autor no puede tener más de 150 caracteres.")]
    public string Autor { get; set; } = string.Empty;

    /// <example>Ingeniería de software</example>
    [Required(ErrorMessage = "Escribe la categoría.")]
    [StringLength(80, ErrorMessage = "La categoría no puede tener más de 80 caracteres.")]
    public string Categoria { get; set; } = string.Empty;
}