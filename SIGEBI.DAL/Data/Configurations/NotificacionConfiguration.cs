using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Notificaciones;

namespace SIGEBI.DAL.Data.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificacion", "Notificaciones");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.TipoEvento).HasMaxLength(40).IsRequired();
        builder.Property(n => n.Mensaje).HasMaxLength(300).IsRequired();
        builder.Property(n => n.FechaEnvio).HasDefaultValueSql("SYSDATETIME()");

        // No se avisa dos veces el mismo vencimiento de un préstamo
        builder.HasIndex(n => n.PrestamoId)
            .IsUnique()
            .HasFilter("[TipoEvento] = 'ProximoVencimiento'")
            .HasDatabaseName("UX_Notificacion_VencimientoUnico");

        builder.HasOne(n => n.Usuario)
            .WithMany(u => u.Notificaciones)
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Prestamo)
            .WithMany(p => p.Notificaciones)
            .HasForeignKey(n => n.PrestamoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
