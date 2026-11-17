namespace SIGEBI.Api.Respuestas;

// Solo describe en Swagger la forma de las respuestas. Las respuestas reales son ResultadoOperacion, con los mismos campos.
public class RespuestaApi<T>
{
    /// <summary>Indica si la operación se realizó.</summary>
    public bool Exito { get; set; }

    /// <summary>Explica lo que pasó, o el motivo del rechazo.</summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Lo que devuelve la operación (si devuelve algo).</summary>
    public T? Datos { get; set; }
}