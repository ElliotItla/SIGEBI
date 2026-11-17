namespace SIGEBI.Api.Respuestas;

// Solo describe en Swagger lo que devuelve Auth/Login. La respuesta real la arma el sistema de tokens de ASP.NET Core.
public class SesionRespuesta
{
    /// <summary>Siempre "Bearer".</summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>El token. Se envía en el encabezado Authorization de cada petición: "Bearer &lt;token&gt;".</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Segundos que dura el token.</summary>
    public int ExpiresIn { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
}