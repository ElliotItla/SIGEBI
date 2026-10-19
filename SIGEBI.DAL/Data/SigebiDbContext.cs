using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Models;
using SIGEBI.DAL.Models.Auditoria;
using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Models.Configuracion;
using SIGEBI.DAL.Models.Notificaciones;
using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.DAL.Models.Seguridad;

namespace SIGEBI.DAL.Data;

public class SigebiDbContext : DbContext
{
    public SigebiDbContext(DbContextOptions<SigebiDbContext> options) : base(options)
    {
    }

    // Seguridad
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    // Catalogo
    public DbSet<Recurso> Recursos => Set<Recurso>();
    public DbSet<Ejemplar> Ejemplares => Set<Ejemplar>();
    public DbSet<HistorialEstadoEjemplar> HistorialesEstadoEjemplar => Set<HistorialEstadoEjemplar>();

    // Prestamos
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<Penalizacion> Penalizaciones => Set<Penalizacion>();

    // Notificaciones
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    // Auditoria
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();

    // Configuracion
    public DbSet<PoliticaPrestamo> PoliticasPrestamo => Set<PoliticaPrestamo>();
    public DbSet<ParametroSistema> ParametrosSistema => Set<ParametroSistema>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Toma todas las clases de la carpeta Data/Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SigebiDbContext).Assembly);
    }

    // Las fechas de creación y modificación se llenan aquí, en un solo lugar.
    // Quién creó o modificó (UsuarioCreacion, UsuarioModificacion) lo asigna la capa de negocio.
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AplicarAuditoria();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AplicarAuditoria();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AplicarAuditoria()
    {
        var ahora = DateTime.Now;

        foreach (var entrada in ChangeTracker.Entries<AuditEntity>())
        {
            if (entrada.State == EntityState.Added && entrada.Entity.FechaCreacion == default)
            {
                entrada.Entity.FechaCreacion = ahora;
            }
            else if (entrada.State == EntityState.Modified)
            {
                entrada.Entity.FechaModificacion = ahora;
            }
        }
    }
}
