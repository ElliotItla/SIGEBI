using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Configuracion;

namespace SIGEBI.Persistence.Context.Configurations;

public class ParametroSistemaConfiguration : IEntityTypeConfiguration<ParametroSistema>
{
    public void Configure(EntityTypeBuilder<ParametroSistema> builder)
    {
        builder.ToTable("ParametroSistema", "Configuracion");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Clave).HasMaxLength(40).IsRequired();

        builder.HasIndex(p => p.Clave).IsUnique();
    }
}
