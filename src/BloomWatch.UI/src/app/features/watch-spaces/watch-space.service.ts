import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/http/api.service';
import { CreateWatchSpaceRequest, WatchSpaceDetail, WatchSpaceSummary } from './watch-space.model';

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
}
