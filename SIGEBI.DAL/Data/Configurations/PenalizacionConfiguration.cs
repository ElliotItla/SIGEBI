using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Prestamos;

namespace SIGEBI.DAL.Data.Configurations;

public class PenalizacionConfiguration : IEntityTypeConfiguration<Penalizacion>
{
    public void Configure(EntityTypeBuilder<Penalizacion> builder)
    {
        builder.ToTable("Penalizacion", "Prestamos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Descripcion).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Estado).HasMaxLength(10).IsRequired();
        builder.Property(p => p.MotivoResolucion).HasMaxLength(200);

        // Un préstamo genera como máximo una penalización
        builder.HasIndex(p => p.PrestamoId)
            .IsUnique()
            .HasDatabaseName("UX_Penalizacion_Prestamo");

        builder.HasOne(p => p.Prestamo)
            .WithOne(pr => pr.Penalizacion)
            .HasForeignKey<Penalizacion>(p => p.PrestamoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Penalizaciones)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ResueltaPor)
            .WithMany()
            .HasForeignKey(p => p.ResueltaPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
