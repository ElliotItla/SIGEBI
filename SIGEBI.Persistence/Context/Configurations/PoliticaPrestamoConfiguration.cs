using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Configuracion;

namespace SIGEBI.Persistence.Context.Configurations;

public class PoliticaPrestamoConfiguration : IEntityTypeConfiguration<PoliticaPrestamo>
{
    public void Configure(EntityTypeBuilder<PoliticaPrestamo> builder)
    {
        builder.ToTable("PoliticaPrestamo", "Configuracion");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TipoUsuario).HasMaxLength(25).IsRequired();

        builder.HasIndex(p => p.TipoUsuario).IsUnique();
    }
}
