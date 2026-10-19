using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGEBI.DAL.Models.Auditoria;

namespace SIGEBI.DAL.Data.Configurations;

public class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("RegistroAuditoria", "Auditoria");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Modulo).HasMaxLength(30).IsRequired();
        builder.Property(r => r.TipoAccion).HasMaxLength(60).IsRequired();
        builder.Property(r => r.Detalle).HasMaxLength(300);
        builder.Property(r => r.Resultado).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Fecha).HasDefaultValueSql("SYSDATETIME()");

        builder.HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
