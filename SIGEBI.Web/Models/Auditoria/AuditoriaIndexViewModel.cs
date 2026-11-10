using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Auditoria;

namespace SIGEBI.Web.Models.Auditoria;

public class AuditoriaIndexViewModel
{
    // Cuántos registros se muestran en pantalla. El archivo CSV trae todos.
    public const int MaximoEnPantalla = 500;

    public static readonly string[] ModulosDisponibles =
    {
        Modulos.Usuarios,
        Modulos.Catalogo,
        Modulos.Prestamos,
        Modulos.Devoluciones,
        Modulos.Penalizaciones,
        Modulos.Notificaciones,
        Modulos.Reportes
    };

    public int? UsuarioId { get; set; }
    public string? Modulo { get; set; }
    public string? TipoAccion { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    // Total que cumple los filtros y la parte que se muestra
    public int Total { get; set; }
    public List<AuditoriaDto> Registros { get; set; } = new();
}