using BloomWatch.Modules.Analytics.Infrastructure.CrossModule;
using BloomWatch.Modules.AnimeTracking.Infrastructure.CrossModule;
using BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence;
using BloomWatch.Modules.Identity.Infrastructure.Persistence;
using BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AnalyticsIdentityReadDbContext = BloomWatch.Modules.Analytics.Infrastructure.CrossModule.IdentityReadDbContext;
using AnalyticsMembershipReadDbContext = BloomWatch.Modules.Analytics.Infrastructure.CrossModule.WatchSpaceMembershipReadDbContext;
using AnimeTrackingMembershipReadDbContext = BloomWatch.Modules.AnimeTracking.Infrastructure.CrossModule.WatchSpaceMembershipReadDbContext;
using WatchSpacesIdentityReadDbContext = BloomWatch.Modules.WatchSpaces.Infrastructure.CrossModule.IdentityReadDbContext;

namespace BloomWatch.Modules.Analytics.IntegrationTests;

/// <summary>
/// Uses named SQLite in-memory databases for all modules:
/// - "identity_db": IdentityDbContext + all IdentityReadDbContexts
/// - "watchspaces_db": WatchSpacesDbContext + all WatchSpaceMembershipReadDbContexts
/// - "animetracking_db": AnimeTrackingDbContext + AnimeTrackingReadDbContext
/// - "anilistsync_db": AniListMediaCacheReadDbContext
/// </summary>
public sealed class AnalyticsWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _identityConnection;
    private SqliteConnection? _watchSpacesConnection;
    private SqliteConnection? _animeTrackingConnection;
    private SqliteConnection? _aniListSyncConnection;
    private readonly string _dbSuffix = Guid.NewGuid().ToString("N");

    public async Task InitializeAsync()
    {
        _identityConnection = new SqliteConnection($"Data Source=identity_{_dbSuffix};Mode=Memory;Cache=Shared");
        _watchSpacesConnection = new SqliteConnection($"Data Source=watchspaces_{_dbSuffix};Mode=Memory;Cache=Shared");
        _animeTrackingConnection = new SqliteConnection($"Data Source=animetracking_{_dbSuffix};Mode=Memory;Cache=Shared");
        _aniListSyncConnection = new SqliteConnection($"Data Source=anilistsync_{_dbSuffix};Mode=Memory;Cache=Shared");

        await _identityConnection.OpenAsync();
        await _watchSpacesConnection.OpenAsync();
        await _animeTrackingConnection.OpenAsync();
        await _aniListSyncConnection.OpenAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_identityConnection is not null) await _identityConnection.DisposeAsync();
        if (_watchSpacesConnection is not null) await _watchSpacesConnection.DisposeAsync();
        if (_animeTrackingConnection is not null) await _animeTrackingConnection.DisposeAsync();
        if (_aniListSyncConnection is not null) await _aniListSyncConnection.DisposeAsync();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all production DbContexts
            RemoveDbContext<IdentityDbContext>(services);
            RemoveDbContext<WatchSpacesDbContext>(services);
            RemoveDbContext<AnimeTrackingDbContext>(services);
            RemoveDbContext<WatchSpacesIdentityReadDbContext>(services);
            RemoveDbContext<AnimeTrackingMembershipReadDbContext>(services);
            RemoveDbContext<AniListMediaCacheReadDbContext>(services);
            RemoveDbContext<AnimeTrackingReadDbContext>(services);
            RemoveDbContext<AnalyticsMembershipReadDbContext>(services);
            RemoveDbContext<AnalyticsIdentityReadDbContext>(services);

            // Identity
            services.AddDbContext<IdentityDbContext>(o => o.UseSqlite(_identityConnection!));
            services.AddDbContext<WatchSpacesIdentityReadDbContext>(o => o.UseSqlite(_identityConnection!));
            services.AddDbContext<AnalyticsIdentityReadDbContext>(o => o.UseSqlite(_identityConnection!));

            // WatchSpaces
            services.AddDbContext<WatchSpacesDbContext>(o => o.UseSqlite(_watchSpacesConnection!));
            services.AddDbContext<AnimeTrackingMembershipReadDbContext>(o => o.UseSqlite(_watchSpacesConnection!));
            services.AddDbContext<AnalyticsMembershipReadDbContext>(o => o.UseSqlite(_watchSpacesConnection!));

            // AnimeTracking
            services.AddDbContext<AnimeTrackingDbContext>(o => o.UseSqlite(_animeTrackingConnection!));
            services.AddDbContext<AnimeTrackingReadDbContext>(o => o.UseSqlite(_animeTrackingConnection!));

            // AniListSync
            services.AddDbContext<AniListMediaCacheReadDbContext>(o => o.UseSqlite(_aniListSyncConnection!));
        });

        builder.UseEnvironment("Testing");
    }

    public void EnsureSchemaCreated()
    {
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<WatchSpacesDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<AnimeTrackingDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<AniListMediaCacheReadDbContext>().Database.EnsureCreated();
    }

    public void SeedMediaCache(int aniListMediaId, string title = "Test Anime", int? episodes = 24)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AniListMediaCacheReadDbContext>();
        db.Database.ExecuteSqlRaw(
            "INSERT OR IGNORE INTO media_cache (anilist_media_id, title_english, title_romaji, title_native, cover_image_url, episodes, format, season, season_year) VALUES ({0}, {1}, {2}, NULL, NULL, {3}, 'TV', 'WINTER', 2026)",
            aniListMediaId, title, title, episodes ?? (object)DBNull.Value);
    }

    public void SeedWatchSession(Guid watchSpaceAnimeId, DateTime sessionDateUtc, Guid? createdByUserId = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnimeTrackingDbContext>();
        var id = Guid.NewGuid();
        var userId = createdByUserId ?? Guid.NewGuid();
        db.Database.ExecuteSqlRaw(
            "INSERT INTO watch_sessions (id, watch_space_anime_id, session_date_utc, start_episode, end_episode, notes, created_by_user_id) VALUES ({0}, {1}, {2}, 1, 2, NULL, {3})",
            id, watchSpaceAnimeId, sessionDateUtc.ToString("o"), userId);
    }

    public void SeedParticipantRating(Guid watchSpaceAnimeId, Guid userId, decimal ratingScore)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnimeTrackingDbContext>();
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow.ToString("o");
        db.Database.ExecuteSqlRaw(
            "INSERT INTO participant_entries (id, watch_space_anime_id, user_id, individual_status, episodes_watched, rating_score, rating_notes, last_updated_at_utc) VALUES ({0}, {1}, {2}, 'Backlog', 0, {3}, NULL, {4})",
            id, watchSpaceAnimeId, userId, ratingScore, now);
    }

    private static void RemoveDbContext<T>(IServiceCollection services) where T : DbContext
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<T>) ||
                d.ServiceType == typeof(T) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition().Name.Contains("DbContextOptionsConfiguration") &&
                 d.ServiceType.GenericTypeArguments.Length == 1 &&
                 d.ServiceType.GenericTypeArguments[0] == typeof(T)))
            .ToList();

        foreach (var descriptor in toRemove)
            services.Remove(descriptor);
    }
}
