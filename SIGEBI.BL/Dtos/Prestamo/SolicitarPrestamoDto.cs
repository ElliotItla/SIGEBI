using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Prestamo;

// UsuarioAccionId es el estudiante o docente que pide el préstamo
public class SolicitarPrestamoDto : DtoBase
{
    public int RecursoId { get; set; }
}