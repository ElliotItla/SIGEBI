using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Recursos;

public class CrearRecursoRequest
{
    /// <summary>ISBN u otro identificador único del recurso. Cada ejemplar recibe este código más un número.</summary>
    /// <example>978-0134494166</example>
    [Required(ErrorMessage = "Escribe el código del recurso.")]
    [StringLength(30, ErrorMessage = "El código no puede tener más de 30 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    /// <example>Clean Architecture</example>
    [Required(ErrorMessage = "Escribe el título.")]
    [StringLength(200, ErrorMessage = "El título no puede tener más de 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <example>Robert C. Martin</example>
    [Required(ErrorMessage = "Escribe el autor.")]
    [StringLength(150, ErrorMessage = "El autor no puede tener más de 150 caracteres.")]
    public string Autor { get; set; } = string.Empty;

    /// <example>Programación</example>
    [Required(ErrorMessage = "Escribe la categoría.")]
    [StringLength(80, ErrorMessage = "La categoría no puede tener más de 80 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Cuántos ejemplares físicos hay. Todos quedan Disponibles.</summary>
    /// <example>3</example>
    [Range(1, 100, ErrorMessage = "La cantidad de ejemplares debe estar entre 1 y 100.")]
    public int CantidadEjemplares { get; set; }
}