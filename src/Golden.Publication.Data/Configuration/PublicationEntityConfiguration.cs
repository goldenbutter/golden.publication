using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PublicationModel = Golden.Publication.Data.Models.Publication;

namespace Golden.Publication.Data.Configuration;

internal sealed class PublicationEntityConfiguration : IEntityTypeConfiguration<PublicationModel>
{
    public void Configure(EntityTypeBuilder<PublicationModel> builder)
    {
        builder.ToTable("publications");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(p => p.PublicationType).HasColumnName("publication_type").IsRequired();
        builder.Property(p => p.Title).HasColumnName("title").IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").IsRequired();
        builder.Property(p => p.Isbn).HasColumnName("isbn").IsRequired();

        builder.HasMany(p => p.Versions)
               .WithOne()
               .HasForeignKey(v => v.PublicationGuid)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
