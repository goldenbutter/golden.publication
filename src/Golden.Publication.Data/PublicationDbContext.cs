using Golden.Publication.Data.Configuration;
using Microsoft.EntityFrameworkCore;
using PublicationModel = Golden.Publication.Data.Models.Publication;
using PublicationVersionModel = Golden.Publication.Data.Models.PublicationVersion;

namespace Golden.Publication.Data;

public sealed class PublicationDbContext : DbContext
{
    public PublicationDbContext(DbContextOptions<PublicationDbContext> options) : base(options) { }

    public DbSet<PublicationModel> Publications => Set<PublicationModel>();
    public DbSet<PublicationVersionModel> PublicationVersions => Set<PublicationVersionModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PublicationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PublicationVersionEntityConfiguration());
    }
}
