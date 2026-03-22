using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Golden.Publication.Data;

// Used only by `dotnet ef` CLI tooling — never instantiated at runtime.
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PublicationDbContext>
{
    public PublicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Publications")
            ?? "Host=localhost;Database=golden_publications;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<PublicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PublicationDbContext(optionsBuilder.Options);
    }
}
