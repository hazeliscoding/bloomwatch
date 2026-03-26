import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomBadgeComponent } from '../../shared/ui/badge/bloom-badge';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomCompatRingComponent } from '../../shared/ui/compat-ring/bloom-compat-ring';
import { WatchSpaceService } from './watch-space.service';
import {
  DashboardCurrentlyWatchingItem,
  DashboardSummary,
} from './watch-space.model';

@Component({
  selector: 'app-watch-space-dashboard',
  standalone: true,
  imports: [RouterLink, BloomCardComponent, BloomBadgeComponent, BloomButtonComponent, BloomCompatRingComponent],
  templateUrl: './watch-space-dashboard.html',
  styleUrl: './watch-space-dashboard.scss',
})
export class WatchSpaceDashboard implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly watchSpaceService = inject(WatchSpaceService);

  private readonly spaceId = this.route.snapshot.paramMap.get('id') ?? '';

  readonly dashboard = signal<DashboardSummary | null>(null);
  readonly isLoading = signal(true);
  readonly loadError = signal('');

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading.set(true);
    this.loadError.set('');
    this.watchSpaceService.getDashboard(this.spaceId).subscribe({
      next: (data) => {
        this.dashboard.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.loadError.set('Failed to load dashboard. Please try again.');
      },
    });
  }

  progressPercent(item: DashboardCurrentlyWatchingItem): number {
    if (!item.episodeCountSnapshot || item.episodeCountSnapshot === 0) return 0;
    return Math.min(100, (item.sharedEpisodesWatched / item.episodeCountSnapshot) * 100);
  }

  formatEpisodeLabel(item: DashboardCurrentlyWatchingItem): string {
    const watched = item.sharedEpisodesWatched;
    const total = item.episodeCountSnapshot;
    return total != null ? `Ep ${watched} / ${total}` : `Ep ${watched}`;
  }

  navigateToAnime(animeId: string): void {
    this.router.navigate(['/watch-spaces', this.spaceId, 'anime', animeId]);
  }

  navigateToManage(): void {
    this.router.navigate(['/watch-spaces', this.spaceId, 'manage']);
  }

  navigateToSettings(): void {
    this.router.navigate(['/watch-spaces', this.spaceId, 'settings']);
  }

  navigateToAnalytics(): void {
    this.router.navigate(['/watch-spaces', this.spaceId, 'analytics']);
  }
}
