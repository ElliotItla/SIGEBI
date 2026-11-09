using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Web.Models.Catalogo;

// Sirve para registrar y para editar un recurso
public class RecursoFormViewModel
{
    public int Id { get; set; }

    // Solo se escribe al registrar. Después no se cambia.
    [StringLength(30, ErrorMessage = "El código no puede tener más de 30 caracteres.")]
    [Display(Name = "Código (ISBN u otro identificador)")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "Escribe el título.")]
    [StringLength(200, ErrorMessage = "El título no puede tener más de 200 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el autor.")]
    [StringLength(150, ErrorMessage = "El autor no puede tener más de 150 caracteres.")]
    [Display(Name = "Autor")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe la categoría.")]
    [StringLength(80, ErrorMessage = "La categoría no puede tener más de 80 caracteres.")]
    [Display(Name = "Categoría")]
    public string Categoria { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "La cantidad de ejemplares debe estar entre 1 y 100.")]
    [Display(Name = "Cantidad de ejemplares")]
    public int CantidadEjemplares { get; set; } = 1;
}