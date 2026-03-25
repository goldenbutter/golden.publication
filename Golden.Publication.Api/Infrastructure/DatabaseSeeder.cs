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
        await EnsureDefaultAdminUserAsync(context, logger);

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

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS users (
                id UUID NOT NULL,
                username TEXT NOT NULL,
                password_hash TEXT NOT NULL,
                role TEXT NOT NULL,
                is_active BOOLEAN NOT NULL,
                created_at TIMESTAMPTZ NOT NULL,
                updated_at TIMESTAMPTZ NOT NULL,
                last_login_at TIMESTAMPTZ NULL,
                CONSTRAINT ""PK_users"" PRIMARY KEY (id)
            );");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_users_username""
            ON users(username);");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS refresh_tokens (
                id UUID NOT NULL,
                user_id UUID NOT NULL,
                token_hash TEXT NOT NULL,
                expires_at TIMESTAMPTZ NOT NULL,
                revoked_at TIMESTAMPTZ NULL,
                replaced_by_token_hash TEXT NULL,
                created_at TIMESTAMPTZ NOT NULL,
                created_by_ip TEXT NULL,
                revoked_by_ip TEXT NULL,
                CONSTRAINT ""PK_refresh_tokens"" PRIMARY KEY (id),
                CONSTRAINT ""FK_refresh_tokens_users_user_id""
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_refresh_tokens_token_hash""
            ON refresh_tokens(token_hash);");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS ""IX_refresh_tokens_user_id""
            ON refresh_tokens(user_id);");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS ""IX_refresh_tokens_expires_at""
            ON refresh_tokens(expires_at);");

        var alreadyApplied = await context.Database
            .SqlQueryRaw<int>($@"
                SELECT 1 AS ""Value""
                FROM ""__EFMigrationsHistory""
                WHERE ""MigrationId"" = '{MigrationId}'")
            .AnyAsync();

        // Record migration as applied so `dotnet ef` sees it as already done
        if (!alreadyApplied)
        {
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                VALUES ('{MigrationId}', '{EfProductVersion}');");
        }

        logger.LogInformation("[Seeder] Schema created successfully.");
    }

    private static async Task EnsureDefaultAdminUserAsync(PublicationDbContext context, ILogger logger)
    {
        var existing = await context.Users.AnyAsync(x => x.Username.ToLower() == "admin");
        if (existing)
            return;

        var hasher = new PasswordHasher();
        var now = DateTimeOffset.UtcNow;
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = hasher.Hash("admin"),
            Role = "admin",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            LastLoginAt = null
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
        logger.LogInformation("[Seeder] Default admin user created (username: admin).");
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
