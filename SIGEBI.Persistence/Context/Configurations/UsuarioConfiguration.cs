using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Seguridad;

namespace SIGEBI.Persistence.Context.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario", "Seguridad");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Identificador).HasMaxLength(30).IsRequired();
        builder.Property(u => u.Nombre).HasMaxLength(80).IsRequired();
        builder.Property(u => u.Apellido).HasMaxLength(80).IsRequired();
        builder.Property(u => u.Correo).HasMaxLength(120).IsRequired();
        builder.Property(u => u.ContrasenaHash).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Tipo).HasMaxLength(25).IsRequired();

        builder.HasIndex(u => u.Identificador).IsUnique();
        builder.HasIndex(u => u.Correo).IsUnique();
    }
}
