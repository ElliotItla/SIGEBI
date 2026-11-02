namespace SIGEBI.BL.Base;

public static class Texto
{
    // Corta un texto al largo máximo de su columna en la base de datos
    public static string Recortar(string texto, int maximo)
    {
        return texto.Length <= maximo ? texto : texto.Substring(0, maximo);
    }
}