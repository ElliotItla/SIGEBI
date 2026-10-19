using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Catalogo;

namespace SIGEBI.DAL.Data.Configurations;

public class HistorialEstadoEjemplarConfiguration : IEntityTypeConfiguration<HistorialEstadoEjemplar>
{
    public void Configure(EntityTypeBuilder<HistorialEstadoEjemplar> builder)
    {
        builder.ToTable("HistorialEstadoEjemplar", "Catalogo");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.EstadoAnterior).HasMaxLength(20);
        builder.Property(h => h.EstadoNuevo).HasMaxLength(20).IsRequired();
        builder.Property(h => h.Motivo).HasMaxLength(200);
        builder.Property(h => h.Fecha).HasDefaultValueSql("SYSDATETIME()");

        builder.HasOne(h => h.Ejemplar)
            .WithMany(e => e.Historial)
            .HasForeignKey(h => h.EjemplarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
