namespace SIGEBI.Api.Requests.Usuarios;

public class CambiarEstadoUsuarioRequest
{
    /// <summary>true activa la cuenta; false la desactiva. Desactivar no borra su historial.</summary>
    /// <example>false</example>
    public bool Activo { get; set; }
}