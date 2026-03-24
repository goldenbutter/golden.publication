using Golden.Publication.Data.Configuration;
using Microsoft.EntityFrameworkCore;
using PublicationModel = Golden.Publication.Data.Models.Publication;
using PublicationVersionModel = Golden.Publication.Data.Models.PublicationVersion;
using RefreshTokenModel = Golden.Publication.Data.Models.RefreshToken;
using UserModel = Golden.Publication.Data.Models.User;

namespace Golden.Publication.Data;

public sealed class PublicationDbContext : DbContext
{
    public PublicationDbContext(DbContextOptions<PublicationDbContext> options) : base(options) { }

    public DbSet<PublicationModel> Publications => Set<PublicationModel>();
    public DbSet<PublicationVersionModel> PublicationVersions => Set<PublicationVersionModel>();
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<RefreshTokenModel> RefreshTokens => Set<RefreshTokenModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PublicationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PublicationVersionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenEntityConfiguration());
    }
}
