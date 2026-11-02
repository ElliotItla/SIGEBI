namespace SIGEBI.BL.Base;

// Valores fijos del sistema: se escriben aquí una sola vez para no repetir el texto ni equivocarse.

public static class TiposUsuario
{
    public const string Estudiante = "Estudiante";
    public const string Docente = "Docente";
    public const string PersonalBibliotecario = "PersonalBibliotecario";
    public const string Administrador = "Administrador";
    public const string Auditor = "Auditor";
}

public static class EstadosEjemplar
{
    public const string Disponible = "Disponible";
    public const string Prestado = "Prestado";
    public const string Reservado = "Reservado";
    public const string FueraDeServicio = "FueraDeServicio";
}

public static class EstadosPrestamo
{
    public const string Pendiente = "Pendiente";
    public const string Rechazado = "Rechazado";
    public const string Activo = "Activo";
    public const string Devuelto = "Devuelto";
}

public static class EstadosPenalizacion
{
    public const string Activa = "Activa";
    public const string Resuelta = "Resuelta";
}

public static class TiposEvento
{
    public const string ProximoVencimiento = "ProximoVencimiento";
    public const string PenalizacionGenerada = "PenalizacionGenerada";
    public const string PrestamoSolicitado = "PrestamoSolicitado";
    public const string PrestamoConfirmado = "PrestamoConfirmado";
    public const string PrestamoDevuelto = "PrestamoDevuelto";
    public const string PenalizacionResuelta = "PenalizacionResuelta";
}

public static class ResultadosAuditoria
{
    public const string Exitoso = "Exitoso";
    public const string Rechazado = "Rechazado";
    public const string Error = "Error";
}

public static class Modulos
{
    public const string Usuarios = "Usuarios";
    public const string Catalogo = "Catalogo";
    public const string Prestamos = "Prestamos";
    public const string Devoluciones = "Devoluciones";
    public const string Penalizaciones = "Penalizaciones";
    public const string Notificaciones = "Notificaciones";
    public const string Reportes = "Reportes";
}