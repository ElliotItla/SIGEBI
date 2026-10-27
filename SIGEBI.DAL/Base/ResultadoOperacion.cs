namespace SIGEBI.DAL.Base;

// Resultado que devuelven los repositorios y los servicios:
// si funcionó, un mensaje claro y, si aplica, un dato.
public class ResultadoOperacion
{
    public bool Exito { get; set; } = true;
    public string Mensaje { get; set; } = string.Empty;
    public object? Datos { get; set; }
}