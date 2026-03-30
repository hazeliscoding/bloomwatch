import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomBadgeComponent } from '../../shared/ui/badge/bloom-badge';
import { BloomAvatarComponent, BloomAvatarStackComponent } from '../../shared/ui/avatar/bloom-avatar';
import { HomeService } from './home.service';
import { HomeOverviewResponse, HomeRecentActivity } from './home.model';
import { BloomBadgeColor } from '../../shared/ui/badge/bloom-badge';

@Component({
  selector: 'app-home',
  imports: [
    DatePipe,
    BloomCardComponent,
    BloomButtonComponent,
    BloomBadgeComponent,
    BloomAvatarComponent,
    BloomAvatarStackComponent,
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit {
  private readonly homeService = inject(HomeService);
  private readonly router = inject(Router);

  readonly data = signal<HomeOverviewResponse | null>(null);
  readonly isLoading = signal(true);
  readonly loadError = signal('');

  ngOnInit(): void {
    this.loadOverview();
  }

  retry(): void {
    this.loadOverview();
  }

  navigateToWatchSpace(id: string): void {
    this.router.navigate(['/watch-spaces', id]);
  }

  navigateToAnimeDetail(activity: HomeRecentActivity): void {
    this.router.navigate(['/watch-spaces', activity.watchSpaceId, 'anime', activity.watchSpaceAnimeId]);
  }

  navigateToCreateWatchSpace(): void {
    this.router.navigate(['/watch-spaces']);
  }

  navigateToBrowseAnime(): void {
    this.router.navigate(['/watch-spaces']);
  }

  roleBadgeColor(role: string): BloomBadgeColor {
    return role === 'Owner' ? 'pink' : 'blue';
  }

  statusBadgeColor(status: string): BloomBadgeColor {
    switch (status) {
      case 'Watching': return 'green';
      case 'Finished': return 'blue';
      case 'Backlog': return 'lilac';
      case 'OnHold': return 'yellow';
      case 'Dropped': return 'neutral';
      default: return 'neutral';
    }
  }

  relativeTime(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins}m ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString();
  }

  private loadOverview(): void {
    this.isLoading.set(true);
    this.loadError.set('');

    this.homeService.getOverview().subscribe({
      next: (overview) => {
        this.data.set(overview);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.loadError.set('Something went wrong');
      },
    });
  }
}
