using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Devoluciones;

public class RegistrarDevolucionRequest
{
    /// <summary>
    /// Código del ejemplar que se devuelve: el código del recurso más un número (por ejemplo 978-0132350884-002).
    /// No es el código del recurso. Se ve en el detalle del recurso y en los préstamos.
    /// </summary>
    /// <example>978-0132350884-002</example>
    [Required(ErrorMessage = "Escribe el código del ejemplar que se devuelve.")]
    [StringLength(40, ErrorMessage = "El código del ejemplar no puede tener más de 40 caracteres.")]
    public string CodigoEjemplar { get; set; } = string.Empty;

    /// <summary>true si el ejemplar llega con daños: queda Fuera de servicio hasta que se revise.</summary>
    /// <example>false</example>
    public bool Danado { get; set; }

    /// <summary>Comentario opcional sobre el estado en que llegó.</summary>
    /// <example>Entregado en buen estado</example>
    [StringLength(300, ErrorMessage = "La observación no puede tener más de 300 caracteres.")]
    public string? Observacion { get; set; }
}