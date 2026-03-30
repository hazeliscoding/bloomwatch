export interface HomeOverviewResponse {
  displayName: string;
  stats: HomeStats;
  watchSpaces: HomeWatchSpaceSummary[];
  recentActivity: HomeRecentActivity[];
}

export interface HomeStats {
  watchSpaceCount: number;
  totalAnimeTracked: number;
  totalEpisodesWatchedTogether: number;
}

export interface HomeWatchSpaceSummary {
  watchSpaceId: string;
  name: string;
  role: string;
  memberCount: number;
  memberPreviews: HomeMemberPreview[];
  watchingCount: number;
  backlogCount: number;
}

export interface HomeMemberPreview {
  displayName: string;
}

export interface HomeRecentActivity {
  watchSpaceAnimeId: string;
  watchSpaceId: string;
  watchSpaceName: string;
  preferredTitle: string;
  coverImageUrl: string | null;
  sharedStatus: string;
  lastUpdatedAt: string;
}
