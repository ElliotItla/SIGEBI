using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Recursos;

public class CambiarEstadoEjemplarRequest
{
    /// <summary>Disponible, Reservado o FueraDeServicio. "Prestado" solo cambia con préstamos y devoluciones.</summary>
    /// <example>FueraDeServicio</example>
    [Required(ErrorMessage = "Elige el nuevo estado.")]
    public string EstadoNuevo { get; set; } = string.Empty;

    /// <summary>Por qué cambia el estado. Queda en el historial del ejemplar.</summary>
    /// <example>Tapa dañada</example>
    [Required(ErrorMessage = "Escribe el motivo del cambio.")]
    [StringLength(200, ErrorMessage = "El motivo no puede tener más de 200 caracteres.")]
    public string Motivo { get; set; } = string.Empty;
}