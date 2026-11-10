using SIGEBI.BL.Base;

namespace SIGEBI.Web.Models.Notificaciones;

// Nombres de los tipos de aviso, como se muestran en pantalla
public static class TiposDeAviso
{
    public static readonly string[] Todos =
    {
        TiposEvento.ProximoVencimiento,
        TiposEvento.PrestamoSolicitado,
        TiposEvento.PrestamoConfirmado,
        TiposEvento.PrestamoDevuelto,
        TiposEvento.PenalizacionGenerada,
        TiposEvento.PenalizacionResuelta
    };

    public static string Nombre(string tipo)
    {
        return tipo switch
        {
            TiposEvento.ProximoVencimiento => "Próximo vencimiento",
            TiposEvento.PrestamoSolicitado => "Préstamo solicitado",
            TiposEvento.PrestamoConfirmado => "Préstamo confirmado",
            TiposEvento.PrestamoDevuelto => "Préstamo devuelto",
            TiposEvento.PenalizacionGenerada => "Penalización generada",
            TiposEvento.PenalizacionResuelta => "Penalización resuelta",
            _ => tipo
        };
    }
}