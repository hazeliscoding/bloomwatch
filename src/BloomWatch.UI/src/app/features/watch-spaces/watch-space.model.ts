export interface WatchSpaceSummary {
  watchSpaceId: string;
  name: string;
  createdAt: string;
  role: string;
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
