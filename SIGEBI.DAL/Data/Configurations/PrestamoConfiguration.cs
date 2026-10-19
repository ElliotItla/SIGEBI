using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Prestamos;

namespace SIGEBI.DAL.Data.Configurations;

public class PrestamoConfiguration : IEntityTypeConfiguration<Prestamo>
{
    public void Configure(EntityTypeBuilder<Prestamo> builder)
    {
        builder.ToTable("Prestamo", "Prestamos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Estado).HasMaxLength(15).IsRequired();
        builder.Property(p => p.MotivoRechazo).HasMaxLength(200);
        builder.Property(p => p.ObservacionDevolucion).HasMaxLength(300);

        // Un mismo ejemplar no puede tener dos préstamos activos a la vez
        builder.HasIndex(p => p.EjemplarId)
            .IsUnique()
            .HasFilter("[Estado] = 'Activo'")
            .HasDatabaseName("UX_Prestamo_EjemplarActivo");

        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Prestamos)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Ejemplar)
            .WithMany(e => e.Prestamos)
            .HasForeignKey(p => p.EjemplarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.AprobadoPor)
            .WithMany()
            .HasForeignKey(p => p.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.RecibidoPor)
            .WithMany()
            .HasForeignKey(p => p.RecibidoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
