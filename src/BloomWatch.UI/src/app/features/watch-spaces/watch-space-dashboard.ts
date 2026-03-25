import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomBadgeComponent } from '../../shared/ui/badge/bloom-badge';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { WatchSpaceService } from './watch-space.service';
import {
  DashboardBacklogHighlight,
  DashboardCurrentlyWatchingItem,
  DashboardRatingGapHighlight,
  DashboardSummary,
} from './watch-space.model';

const RING_RADIUS = 56;
const RING_CIRCUMFERENCE = 2 * Math.PI * RING_RADIUS;

@Component({
  selector: 'app-watch-space-dashboard',
  standalone: true,
  imports: [RouterLink, BloomCardComponent, BloomBadgeComponent, BloomButtonComponent],
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

  // Compatibility ring computations
  readonly ringStrokeDashoffset = computed(() => {
    const compat = this.dashboard()?.compatibility;
    if (!compat) return RING_CIRCUMFERENCE;
    const fraction = compat.score / 100;
    return RING_CIRCUMFERENCE * (1 - fraction);
  });

  readonly ringColor = computed(() => {
    const score = this.dashboard()?.compatibility?.score ?? 0;
    if (score >= 80) return 'var(--bloom-green-400)';
    if (score >= 50) return 'var(--bloom-yellow-400)';
    return 'var(--bloom-pink-400)';
  });

  readonly ringCircumference = RING_CIRCUMFERENCE;

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
}
