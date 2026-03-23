export interface MemberPreview {
  displayName: string;
}

export interface WatchSpaceSummary {
  watchSpaceId: string;
  name: string;
  createdAt: string;
  role: string;
  memberCount?: number;
  memberPreviews?: MemberPreview[];
}

export interface CreateWatchSpaceRequest {
  name: string;
}

export interface MemberDetail {
  userId: string;
  displayName: string;
  role: string;
  joinedAt: string;
}

export interface WatchSpaceDetail {
  watchSpaceId: string;
  name: string;
  createdAt: string;
  members: MemberDetail[];
}

export interface InvitationDetail {
  invitationId: string;
  invitedEmail: string;
  status: string;
  expiresAt: string;
  createdAt: string;
}

export interface InviteMemberResponse {
  invitationId: string;
  invitedEmail: string;
  status: string;
  expiresAt: string;
  token: string;
}

export interface InvitationPreview {
  watchSpaceId: string;
  watchSpaceName: string;
  invitedEmail: string;
  status: string;
  expiresAt: string;
}

export interface AcceptInvitationResponse {
  watchSpaceId: string;
}

export interface AnimeSearchResult {
  anilistMediaId: number;
  titleRomaji: string | null;
  titleEnglish: string | null;
  coverImageUrl: string | null;
  episodes: number | null;
  status: string | null;
  format: string | null;
  season: string | null;
  seasonYear: number | null;
  genres: string[];
}

export interface AnimeParticipantSummary {
  userId: string;
  displayName: string;
  individualStatus: string;
  episodesWatched: number;
}

export interface WatchSpaceAnimeListItem {
  watchSpaceAnimeId: string;
  anilistMediaId: number;
  preferredTitle: string;
  coverImageUrlSnapshot: string | null;
  episodeCountSnapshot: number | null;
  sharedStatus: string;
  sharedEpisodesWatched: number;
  addedAtUtc: string;
  participants: AnimeParticipantSummary[];
  formatSnapshot?: string | null;
  seasonSnapshot?: string | null;
  seasonYearSnapshot?: number | null;
  mood?: string | null;
  vibe?: string | null;
}

export interface AddAnimeToWatchSpaceRequest {
  aniListMediaId: number;
  mood?: string | null;
  vibe?: string | null;
  pitch?: string | null;
}

export interface AddAnimeToWatchSpaceResult {
  watchSpaceAnimeId: string;
  preferredTitle: string;
  episodeCountSnapshot: number | null;
  coverImageUrlSnapshot: string | null;
  format: string | null;
  season: string | null;
  seasonYear: number | null;
}

export interface ParticipantDetail {
  userId: string;
  individualStatus: string;
  episodesWatched: number;
  ratingScore: number | null;
  ratingNotes: string | null;
  lastUpdatedAtUtc: string;
}

export interface WatchSessionDetail {
  watchSessionId: string;
  sessionDateUtc: string;
  startEpisode: number;
  endEpisode: number;
  notes: string | null;
  createdByUserId: string;
}

export interface WatchSpaceAnimeDetail {
  watchSpaceAnimeId: string;
  anilistMediaId: number;
  preferredTitle: string;
  coverImageUrlSnapshot: string | null;
  episodeCountSnapshot: number | null;
  format: string | null;
  season: string | null;
  seasonYear: number | null;
  sharedStatus: string;
  sharedEpisodesWatched: number;
  mood: string | null;
  vibe: string | null;
  pitch: string | null;
  addedByUserId: string;
  addedAtUtc: string;
  participants: ParticipantDetail[];
  watchSessions: WatchSessionDetail[];
  genres?: string[];
  anilistScore?: number | null;
  anilistPopularity?: number | null;
  description?: string | null;
}

export interface UpdateSharedAnimeRequest {
  sharedStatus?: string;
  sharedEpisodesWatched?: number;
}

export interface UpdateParticipantProgressRequest {
  individualStatus: string;
  episodesWatched: number;
}

export interface UpdateParticipantRatingRequest {
  ratingScore: number;
  ratingNotes?: string | null;
}

export interface RecordWatchSessionRequest {
  sessionDateUtc: string;
  startEpisode: number;
  endEpisode: number;
  notes?: string | null;
}
