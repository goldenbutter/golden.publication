using Golden.Publication.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Golden.Publication.Data.Configuration;

internal sealed class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.RevokedAt).HasColumnName("revoked_at").HasColumnType("timestamp with time zone");
        builder.Property(x => x.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.CreatedByIp).HasColumnName("created_by_ip");
        builder.Property(x => x.RevokedByIp).HasColumnName("revoked_by_ip");

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAt);

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
