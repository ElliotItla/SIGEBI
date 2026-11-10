using SIGEBI.BL.Base;

namespace SIGEBI.Web.Seguridad;

// Los roles son los tipos de usuario del sistema
public static class Roles
{
    public const string Administrador = TiposUsuario.Administrador;
    public const string Personal = TiposUsuario.PersonalBibliotecario;
    public const string Auditor = TiposUsuario.Auditor;
    public const string Estudiante = TiposUsuario.Estudiante;
    public const string Docente = TiposUsuario.Docente;

    // Combinaciones para [Authorize(Roles = ...)]
    public const string AdminPersonal = Administrador + "," + Personal;
    public const string AdminAuditor = Administrador + "," + Auditor;
    public const string AdminPersonalAuditor = Administrador + "," + Personal + "," + Auditor;
    public const string Lectores = Estudiante + "," + Docente;
}