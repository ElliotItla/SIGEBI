using System.Globalization;

namespace SIGEBI.Web.Models.Reportes;

public static class Formato
{
    // 66.7 se muestra como "66,7 %". Sin dato, una raya.
    public static string Porcentaje(double? valor)
    {
        return valor == null
            ? "—"
            : valor.Value.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ',') + " %";
    }
}