using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Catalogo;

namespace SIGEBI.Persistence.Context.Configurations;

public class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.ToTable("Recurso", "Catalogo");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Codigo).HasMaxLength(30).IsRequired();
        builder.Property(r => r.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Autor).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Categoria).HasMaxLength(80).IsRequired();

        builder.HasIndex(r => r.Codigo).IsUnique();
    }
}
