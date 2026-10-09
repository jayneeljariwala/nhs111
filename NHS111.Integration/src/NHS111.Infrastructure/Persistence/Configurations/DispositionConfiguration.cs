namespace NHS111.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NHS111.Domain.Entities;

public class DispositionConfiguration : IEntityTypeConfiguration<Disposition>
{
    public void Configure(EntityTypeBuilder<Disposition> builder)
    {
        builder.ToTable("dispositions");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.NhsNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.DxCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(d => d.DosCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(d => d.Urgency)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.RoutedTo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired();
    }
}
