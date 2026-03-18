import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/http/api.service';
import { CreateWatchSpaceRequest, WatchSpaceSummary } from './watch-space.model';

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
}
