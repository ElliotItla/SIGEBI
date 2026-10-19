namespace SIGEBI.BL.Base;

// Resultado que devuelven los servicios: si funcionó, un mensaje claro y, si aplica, un dato.
public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}
