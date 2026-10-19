using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Catalogo;

namespace SIGEBI.DAL.Data.Configurations;

public class EjemplarConfiguration : IEntityTypeConfiguration<Ejemplar>
{
    public void Configure(EntityTypeBuilder<Ejemplar> builder)
    {
        builder.ToTable("Ejemplar", "Catalogo");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CodigoEjemplar).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Estado).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.CodigoEjemplar).IsUnique();

        builder.HasOne(e => e.Recurso)
            .WithMany(r => r.Ejemplares)
            .HasForeignKey(e => e.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
