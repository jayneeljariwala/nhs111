namespace NHS111.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NHS111.Domain.Entities;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PerformedAt)
            .IsRequired();

        builder.HasOne<Disposition>()
            .WithMany()
            .HasForeignKey(a => a.DispositionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
