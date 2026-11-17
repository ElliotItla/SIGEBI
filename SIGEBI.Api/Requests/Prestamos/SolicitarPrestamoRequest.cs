using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Prestamos;

public class SolicitarPrestamoRequest
{
    /// <summary>Número del recurso que quieres pedir (sale del catálogo). El sistema te asigna un ejemplar disponible.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Indica el número del recurso que quieres pedir.")]
    public int RecursoId { get; set; }
}