using System.Text.RegularExpressions;

namespace SIGEBI.Web.Base;

// Textos y colores de los estados, para que todas las pantallas los muestren igual
public static class Etiquetas
{
    public static string Clase(string? estado)
    {
        return estado switch
        {
            "Disponible" or "Activo" or "Exitoso" => "etiqueta etiqueta-verde",
            "Pendiente" or "Prestado" or "Reservado" => "etiqueta etiqueta-ambar",
            "Rechazado" or "Activa" or "FueraDeServicio" or "Inactivo" or "Error" => "etiqueta etiqueta-roja",
            "Devuelto" or "Resuelta" => "etiqueta etiqueta-gris",
            _ => "etiqueta"
        };
    }

    // "FueraDeServicio" se muestra como "Fuera de servicio"
    public static string Texto(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
        {
            return string.Empty;
        }

        var separado = Regex.Replace(valor, "(?<=[a-z])(?=[A-Z])", " ");

        return char.ToUpper(separado[0]) + separado[1..].ToLowerInvariant();
    }
}