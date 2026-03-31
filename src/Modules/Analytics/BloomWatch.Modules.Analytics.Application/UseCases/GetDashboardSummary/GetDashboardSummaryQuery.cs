using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

/// <summary>
/// Query to retrieve the composite dashboard summary for a watch space, combining
/// aggregate stats, currently-watching list, backlog highlights, rating gaps, and compatibility.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to summarize.</param>
/// <param name="UserId">The requesting user's identifier (must be a member of the watch space).</param>
public sealed record GetDashboardSummaryQuery(Guid WatchSpaceId, Guid UserId) : IQuery<DashboardSummaryResult>;
