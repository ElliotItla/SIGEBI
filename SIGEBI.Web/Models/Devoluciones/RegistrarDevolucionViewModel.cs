using System.ComponentModel.DataAnnotations;
using SIGEBI.BL.Dtos.Devolucion;

namespace SIGEBI.Web.Models.Devoluciones;

public class RegistrarDevolucionViewModel
{
    [Required(ErrorMessage = "Escribe el código del ejemplar que se devuelve.")]
    [StringLength(40, ErrorMessage = "El código del ejemplar no puede tener más de 40 caracteres.")]
    [Display(Name = "Código del ejemplar")]
    public string CodigoEjemplar { get; set; } = string.Empty;

    [Display(Name = "El ejemplar llegó con daños")]
    public bool Danado { get; set; }

    [StringLength(300, ErrorMessage = "La observación no puede tener más de 300 caracteres.")]
    [Display(Name = "Observación (opcional)")]
    public string? Observacion { get; set; }

    // Resumen de la última devolución registrada, para mostrarlo debajo del formulario
    public DevolucionDto? Resultado { get; set; }
}