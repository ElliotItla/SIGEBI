using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Prestamos;

public class RechazarPrestamoRequest
{
    /// <summary>Por qué se rechaza. Lo ve el usuario que pidió el préstamo.</summary>
    /// <example>El recurso está reservado para una clase</example>
    [Required(ErrorMessage = "Escribe el motivo del rechazo.")]
    [StringLength(200, ErrorMessage = "El motivo no puede tener más de 200 caracteres.")]
    public string Motivo { get; set; } = string.Empty;
}