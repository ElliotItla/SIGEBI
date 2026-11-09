using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Recurso;

namespace SIGEBI.Web.Models.Catalogo;

public class DetalleRecursoViewModel
{
    // Estados a los que el personal puede pasar un ejemplar a mano ("Prestado" cambia solo con préstamos y devoluciones)
    public static readonly string[] EstadosManuales =
    {
        EstadosEjemplar.Disponible,
        EstadosEjemplar.Reservado,
        EstadosEjemplar.FueraDeServicio
    };

    public RecursoDto Recurso { get; set; } = new();

    // Administrador y personal: editar y cambiar el estado de los ejemplares
    public bool PuedeAdministrar { get; set; }

    // Solo el administrador da de baja un recurso
    public bool PuedeDarDeBaja { get; set; }

    public bool PuedeVerHistorial { get; set; }

    // Estudiantes y docentes: pedir el recurso prestado
    public bool PuedeSolicitar { get; set; }
}