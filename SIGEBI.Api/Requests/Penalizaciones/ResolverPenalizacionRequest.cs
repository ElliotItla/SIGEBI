using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Penalizaciones;

public class ResolverPenalizacionRequest
{
    /// <summary>Por qué se resuelve: un pago, el cumplimiento de una condición o una exoneración autorizada.</summary>
    /// <example>Pago en caja</example>
    [Required(ErrorMessage = "Escribe el motivo de la resolución.")]
    [StringLength(200, ErrorMessage = "El motivo no puede tener más de 200 caracteres.")]
    public string Motivo { get; set; } = string.Empty;
}