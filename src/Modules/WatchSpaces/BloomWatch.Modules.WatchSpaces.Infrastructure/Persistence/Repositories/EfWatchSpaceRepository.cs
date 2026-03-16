using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Entities;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="IWatchSpaceRepository"/>,
/// persisting <see cref="WatchSpace"/> aggregates to the <c>watch_spaces</c> PostgreSQL schema.
/// </summary>
/// <remarks>
/// <para>
/// All queries eagerly include child collections (<see cref="WatchSpaceMember"/> and
/// <see cref="Invitation"/>) when the caller needs full aggregate state. Simpler lookups
/// (e.g., <see cref="GetByIdAsync"/>) load only the aggregate root.
/// </para>
/// <para>
/// The <see cref="SaveChangesAsync"/> method contains a workaround for EF Core's change
/// tracking behavior with backing-field collections: it temporarily disables auto-detect
/// changes so that newly added child entities in backing fields are correctly marked as
/// <see cref="EntityState.Added"/> rather than <see cref="EntityState.Modified"/>.
/// </para>
/// </remarks>
/// <param name="dbContext">The <see cref="WatchSpacesDbContext"/> scoped to the current request.</param>
internal sealed class EfWatchSpaceRepository(WatchSpacesDbContext dbContext) : IWatchSpaceRepository
{
    /// <summary>
    /// Retrieves a <see cref="WatchSpace"/> by its identifier without loading child collections.
    /// </summary>
    /// <param name="id">The unique identifier of the watch space.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching <see cref="WatchSpace"/>, or <see langword="null"/> if not found.</returns>
    public Task<WatchSpace?> GetByIdAsync(WatchSpaceId id, CancellationToken cancellationToken = default)
        => dbContext.WatchSpaces.FirstOrDefaultAsync(ws => ws.Id == id, cancellationToken);

    /// <summary>
    /// Retrieves a <see cref="WatchSpace"/> by its identifier, eagerly loading both
    /// <see cref="WatchSpace.Members"/> and <see cref="WatchSpace.Invitations"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the watch space.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="WatchSpace"/> with members and invitations loaded,
    /// or <see langword="null"/> if not found.
    /// </returns>
    public Task<WatchSpace?> GetByIdWithMembersAsync(WatchSpaceId id, CancellationToken cancellationToken = default)
        => dbContext.WatchSpaces
            .Include(ws => ws.Members)
            .Include(ws => ws.Invitations)
            .FirstOrDefaultAsync(ws => ws.Id == id, cancellationToken);

    /// <summary>
    /// Retrieves a <see cref="WatchSpace"/> that owns the invitation with the given token,
    /// eagerly loading members and invitations.
    /// </summary>
    /// <remarks>
    /// Performs a two-step lookup: first resolves the <see cref="WatchSpaceId"/> from the
    /// <see cref="Invitation"/> table directly, then loads the full aggregate.
    /// This avoids issues with EF Core navigation property access on keyless or
    /// inverse-less relationships.
    /// </remarks>
    /// <param name="token">The unique invitation token.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// The owning <see cref="WatchSpace"/> with members and invitations loaded,
    /// or <see langword="null"/> if no invitation matches the token.
    /// </returns>
    public async Task<WatchSpace?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // Find the WatchSpaceId via the Invitation table directly (avoids querying ignored navigation)
        var watchSpaceId = await dbContext.Set<Invitation>()
            .Where(i => i.Token == token)
            .Select(i => i.WatchSpaceId)
            .FirstOrDefaultAsync(cancellationToken);

        if (watchSpaceId == default) return null;

        return await GetByIdWithMembersAsync(watchSpaceId, cancellationToken);
    }

    /// <summary>
    /// Retrieves all <see cref="WatchSpace"/> aggregates where the specified user is a member,
    /// eagerly loading the members collection.
    /// </summary>
    /// <remarks>
    /// Performs a two-step lookup: first collects matching <see cref="WatchSpaceId"/> values from
    /// the <c>watch_space_members</c> table, then loads the full aggregates with members included.
    /// </remarks>
    /// <param name="userId">The user identifier to search for in membership records.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of watch spaces the user belongs to (may be empty).</returns>
    public async Task<IReadOnlyList<WatchSpace>> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Find WatchSpaceIds via the Member table directly (avoids querying ignored navigation)
        var watchSpaceIds = await dbContext.Set<WatchSpaceMember>()
            .Where(m => m.UserId == userId)
            .Select(m => m.WatchSpaceId)
            .ToListAsync(cancellationToken);

        return await dbContext.WatchSpaces
            .Include(ws => ws.Members)
            .Where(ws => watchSpaceIds.Contains(ws.Id))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new <see cref="WatchSpace"/> aggregate to the context and immediately persists it.
    /// </summary>
    /// <param name="watchSpace">The watch space aggregate to persist.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the insert and save have finished.</returns>
    public async Task AddAsync(WatchSpace watchSpace, CancellationToken cancellationToken = default)
    {
        await dbContext.WatchSpaces.AddAsync(watchSpace, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Detects new child entities added via domain operations on tracked aggregates
    /// and persists all pending changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// EF Core's <c>DetectChanges</c> misclassifies entities added to backing-field
    /// collections as <see cref="EntityState.Modified"/> when their keys are non-default.
    /// This method temporarily disables auto-detect, explicitly marks detached children
    /// as <see cref="EntityState.Added"/>, then re-enables detection before saving.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when all changes have been persisted.</returns>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Accessing ChangeTracker.Entries() triggers DetectChanges automatically.
        // DetectChanges classifies entities with non-empty Guid keys found in backing-field
        // collections as "Modified" (assuming they were DB-loaded) rather than "Added".
        // Disabling auto-detect lets us safely iterate and explicitly mark new child
        // entities as Added BEFORE DetectChanges runs.
        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            foreach (var entry in dbContext.ChangeTracker.Entries<WatchSpace>())
            {
                foreach (var member in entry.Entity.Members)
                    if (dbContext.Entry(member).State == EntityState.Detached)
                        dbContext.Set<WatchSpaceMember>().Add(member);

                foreach (var invitation in entry.Entity.Invitations)
                    if (dbContext.Entry(invitation).State == EntityState.Detached)
                        dbContext.Set<Invitation>().Add(invitation);
            }

            dbContext.ChangeTracker.DetectChanges();
        }
        finally
        {
            dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
