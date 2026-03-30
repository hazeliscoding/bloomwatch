import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/http/api.service';
import { HomeOverviewResponse } from './home.model';

@Injectable({ providedIn: 'root' })
export class HomeService {
  private readonly api = inject(ApiService);

  getOverview(): Observable<HomeOverviewResponse> {
    return this.api.get<HomeOverviewResponse>('/home/overview');
  }
}
