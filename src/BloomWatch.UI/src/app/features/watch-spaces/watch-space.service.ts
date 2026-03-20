import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService } from '../../core/http/api.service';
import {
  AcceptInvitationResponse,
  AddAnimeToWatchSpaceRequest,
  AddAnimeToWatchSpaceResult,
  AnimeSearchResult,
  CreateWatchSpaceRequest,
  InvitationDetail,
  InvitationPreview,
  InviteMemberResponse,
  ParticipantDetail,
  RecordWatchSessionRequest,
  UpdateParticipantProgressRequest,
  UpdateParticipantRatingRequest,
  WatchSpaceAnimeDetail,
  WatchSpaceAnimeListItem,
  WatchSpaceDetail,
  WatchSpaceSummary,
} from './watch-space.model';

@Injectable({ providedIn: 'root' })
export class WatchSpaceService {
  private readonly api = inject(ApiService);

  getMyWatchSpaces(): Observable<WatchSpaceSummary[]> {
    return this.api.get<WatchSpaceSummary[]>('/watchspaces');
  }

  createWatchSpace(name: string): Observable<WatchSpaceSummary> {
    const body: CreateWatchSpaceRequest = { name };
    return this.api.post<WatchSpaceSummary>('/watchspaces', body);
  }

  getWatchSpaceById(id: string): Observable<WatchSpaceDetail> {
    return this.api.get<WatchSpaceDetail>(`/watchspaces/${id}`);
  }

  renameWatchSpace(id: string, name: string): Observable<void> {
    return this.api.patch<void>(`/watchspaces/${id}`, { name });
  }

  removeMember(spaceId: string, userId: string): Observable<void> {
    return this.api.delete<void>(`/watchspaces/${spaceId}/members/${userId}`);
  }

  transferOwnership(spaceId: string, newOwnerId: string): Observable<void> {
    return this.api.post<void>(`/watchspaces/${spaceId}/transfer-ownership`, { newOwnerId });
  }

  leaveWatchSpace(spaceId: string): Observable<void> {
    return this.api.delete<void>(`/watchspaces/${spaceId}/members/me`);
  }

  sendInvitation(spaceId: string, email: string): Observable<InviteMemberResponse> {
    return this.api.post<InviteMemberResponse>(`/watchspaces/${spaceId}/invitations`, { email });
  }

  listInvitations(spaceId: string): Observable<InvitationDetail[]> {
    return this.api.get<InvitationDetail[]>(`/watchspaces/${spaceId}/invitations`);
  }

  revokeInvitation(spaceId: string, invitationId: string): Observable<void> {
    return this.api.delete<void>(`/watchspaces/${spaceId}/invitations/${invitationId}`);
  }

  getInvitationPreview(token: string): Observable<InvitationPreview> {
    return this.api.get<InvitationPreview>(`/watchspaces/invitations/${token}`);
  }

  acceptInvitation(token: string): Observable<AcceptInvitationResponse> {
    return this.api.post<AcceptInvitationResponse>(`/watchspaces/invitations/${token}/accept`, {});
  }

  declineInvitation(token: string): Observable<void> {
    return this.api.post<void>(`/watchspaces/invitations/${token}/decline`, {});
  }

  listWatchSpaceAnime(spaceId: string, status?: string): Observable<WatchSpaceAnimeListItem[]> {
    const url = status
      ? `/watchspaces/${spaceId}/anime?status=${encodeURIComponent(status)}`
      : `/watchspaces/${spaceId}/anime`;
    return this.api.get<{ items: WatchSpaceAnimeListItem[] }>(url)
      .pipe(map((res) => res.items));
  }

  searchAnime(query: string): Observable<AnimeSearchResult[]> {
    return this.api.get<AnimeSearchResult[]>(`/api/anilist/search?query=${encodeURIComponent(query)}`);
  }

  ensureMediaCached(anilistMediaId: number): Observable<unknown> {
    return this.api.get<unknown>(`/api/anilist/media/${anilistMediaId}`);
  }

  addAnimeToWatchSpace(spaceId: string, body: AddAnimeToWatchSpaceRequest): Observable<AddAnimeToWatchSpaceResult> {
    return this.api.post<AddAnimeToWatchSpaceResult>(`/watchspaces/${spaceId}/anime`, body);
  }

  getAnimeDetail(spaceId: string, animeId: string): Observable<WatchSpaceAnimeDetail> {
    return this.api.get<WatchSpaceAnimeDetail>(`/watchspaces/${spaceId}/anime/${animeId}`);
  }

  updateParticipantProgress(spaceId: string, animeId: string, body: UpdateParticipantProgressRequest): Observable<ParticipantDetail> {
    return this.api.patch<ParticipantDetail>(`/watchspaces/${spaceId}/anime/${animeId}/participant-progress`, body);
  }

  updateParticipantRating(spaceId: string, animeId: string, body: UpdateParticipantRatingRequest): Observable<ParticipantDetail> {
    return this.api.patch<ParticipantDetail>(`/watchspaces/${spaceId}/anime/${animeId}/participant-rating`, body);
  }

  recordWatchSession(spaceId: string, animeId: string, body: RecordWatchSessionRequest): Observable<{ watchSessionId: string }> {
    return this.api.post<{ watchSessionId: string }>(`/watchspaces/${spaceId}/anime/${animeId}/sessions`, body);
  }
}
