using System.Xml.Serialization;
using Golden.Publication.Data;
using Golden.Publication.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Golden.Publication.Api.Infrastructure;

public static class DatabaseSeeder
{
    private const string MigrationId = "20260322000000_InitialCreate";
    private const string EfProductVersion = "9.0.0";

    public static async Task SeedAsync(PublicationDbContext context, IHostEnvironment env, ILogger logger)
    {
        await EnsureSchemaAsync(context, logger);

        if (await context.Publications.AnyAsync())
        {
            logger.LogInformation("[Seeder] Publications table already populated — skipping seed.");
            return;
        }

        var xmlPath = ResolveXmlPath(env);
        if (xmlPath is null)
        {
            logger.LogWarning("[Seeder] publications.xml not found — skipping seed. Set PUBLICATION_XML_FILE env var.");
            return;
        }

        logger.LogInformation("[Seeder] Seeding from {Path}", xmlPath);

        var serializer = new XmlSerializer(typeof(PublicationsDocument));
        await using var stream = File.OpenRead(xmlPath);
        var document = (PublicationsDocument)serializer.Deserialize(stream)!;

        context.Publications.AddRange(document.Items);
        await context.SaveChangesAsync();

        logger.LogInformation("[Seeder] Inserted {Count} publications.", document.Items.Count);
    }

    private static async Task EnsureSchemaAsync(PublicationDbContext context, ILogger logger)
    {
        // Create migrations history table if it doesn't exist
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                ""MigrationId"" character varying(150) NOT NULL,
                ""ProductVersion"" character varying(32) NOT NULL,
                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
            );");

        // Skip schema creation if migration already recorded as applied
        var alreadyApplied = await context.Database
            .SqlQueryRaw<int>($@"
                SELECT 1 AS ""Value""
                FROM ""__EFMigrationsHistory""
                WHERE ""MigrationId"" = '{MigrationId}'")
            .AnyAsync();

        if (alreadyApplied)
        {
            logger.LogInformation("[Seeder] Migration {Id} already applied.", MigrationId);
            return;
        }

        logger.LogInformation("[Seeder] Applying schema for migration {Id}.", MigrationId);

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS publications (
                id UUID NOT NULL,
                publication_type TEXT NOT NULL,
                title TEXT NOT NULL,
                description TEXT NOT NULL,
                isbn TEXT NOT NULL,
                CONSTRAINT ""PK_publications"" PRIMARY KEY (id)
            );");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS publication_versions (
                id UUID NOT NULL,
                publication_guid UUID NOT NULL,
                version TEXT NOT NULL,
                language TEXT NOT NULL,
                cover_title TEXT NOT NULL,
                CONSTRAINT ""PK_publication_versions"" PRIMARY KEY (id),
                CONSTRAINT ""FK_publication_versions_publications_publication_guid""
                    FOREIGN KEY (publication_guid) REFERENCES publications(id) ON DELETE CASCADE
            );");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS ""IX_publication_versions_publication_guid""
            ON publication_versions(publication_guid);");

        // Record migration as applied so `dotnet ef` sees it as already done
        await context.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
            VALUES ('{MigrationId}', '{EfProductVersion}');");

        logger.LogInformation("[Seeder] Schema created successfully.");
    }

    private static string? ResolveXmlPath(IHostEnvironment env)
    {
        var fromEnv = Environment.GetEnvironmentVariable("PUBLICATION_XML_FILE");
        if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
            return fromEnv;

        var solutionRoot = Path.GetFullPath(Path.Combine(env.ContentRootPath, ".."));
        var fallback = Path.Combine(solutionRoot, "src", "Golden.Publication.Data", "Data", "publications.xml");
        return File.Exists(fallback) ? fallback : null;
    }
}
