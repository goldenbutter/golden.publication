using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PublicationVersionModel = Golden.Publication.Data.Models.PublicationVersion;

namespace Golden.Publication.Data.Configuration;

internal sealed class PublicationVersionEntityConfiguration : IEntityTypeConfiguration<PublicationVersionModel>
{
    public void Configure(EntityTypeBuilder<PublicationVersionModel> builder)
    {
        builder.ToTable("publication_versions");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(v => v.PublicationGuid).HasColumnName("publication_guid").HasColumnType("uuid").IsRequired();
        builder.Property(v => v.Version).HasColumnName("version").IsRequired();
        builder.Property(v => v.Language).HasColumnName("language").IsRequired();
        builder.Property(v => v.CoverTitle).HasColumnName("cover_title").IsRequired();
    }
}
