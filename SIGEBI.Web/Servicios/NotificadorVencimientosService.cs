using SIGEBI.BL.Interfaces;

namespace SIGEBI.Web.Servicios;

// Tarea en segundo plano: cada cierto tiempo avisa a los usuarios cuyos préstamos están por vencer.
// Se registra sola (AddSigebiWeb busca las tareas de esta carpeta).
public class NotificadorVencimientosService : BackgroundService
{
    private readonly IServiceScopeFactory _fabrica;
    private readonly ILogger<NotificadorVencimientosService> _registro;
    private readonly TimeSpan _retardoInicial;
    private readonly TimeSpan _intervalo;

    public NotificadorVencimientosService(
        IServiceScopeFactory fabrica,
        IConfiguration configuracion,
        ILogger<NotificadorVencimientosService> registro)
    {
        _fabrica = fabrica;
        _registro = registro;

        // Se puede ajustar en appsettings.json, sección "Notificaciones"
        _retardoInicial = TimeSpan.FromSeconds(Math.Max(1, configuracion.GetValue("Notificaciones:RetardoInicialSegundos", 30)));
        _intervalo = TimeSpan.FromMinutes(Math.Max(1, configuracion.GetValue("Notificaciones:IntervaloMinutos", 60)));
    }

    protected override async Task ExecuteAsync(CancellationToken cancelacion)
    {
        try
        {
            // Un momento para que la aplicación termine de arrancar
            await Task.Delay(_retardoInicial, cancelacion);

            using var temporizador = new PeriodicTimer(_intervalo);

            do
            {
                await EjecutarAsync();
            }
            while (await temporizador.WaitForNextTickAsync(cancelacion));
        }
        catch (OperationCanceledException)
        {
            // La aplicación se está cerrando
        }
    }

    private async Task EjecutarAsync()
    {
        try
        {
            // El servicio es "scoped": cada ejecución usa el suyo
            using var alcance = _fabrica.CreateScope();
            var servicio = alcance.ServiceProvider.GetRequiredService<INotificacionServicio>();

            var resultado = await servicio.NotificarProximosVencimientosAsync();

            _registro.LogInformation("Avisos de vencimiento: {Mensaje}", resultado.Mensaje);
        }
        catch (Exception error)
        {
            // Un fallo no debe tumbar la aplicación: se registra y se reintenta en la próxima vuelta
            _registro.LogError(error, "No se pudieron enviar los avisos de vencimiento.");
        }
    }
}