import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomCompatRingComponent } from '../../shared/ui/compat-ring/bloom-compat-ring';
import { WatchSpaceService } from './watch-space.service';
import {
  CompatibilityScoreResult,
  RatingGapsResult,
  SharedStatsResult,
} from './watch-space.model';

@Component({
  selector: 'app-watch-space-analytics',
  standalone: true,
  imports: [DatePipe, BloomCardComponent, BloomCompatRingComponent, BaseChartDirective],
  templateUrl: './watch-space-analytics.html',
  styleUrl: './watch-space-analytics.scss',
})
export class WatchSpaceAnalytics implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly watchSpaceService = inject(WatchSpaceService);

  private readonly spaceId = this.route.snapshot.paramMap.get('id') ?? '';

  // Section data
  readonly compatData = signal<CompatibilityScoreResult | null>(null);
  readonly gapsData = signal<RatingGapsResult | null>(null);
  readonly statsData = signal<SharedStatsResult | null>(null);

  // Loading states
  readonly isLoading = signal(true);
  readonly compatError = signal('');
  readonly gapsError = signal('');
  readonly statsError = signal('');

  // Chart config computed from rating gaps
  readonly chartData = computed<ChartConfiguration<'bar'>['data'] | null>(() => {
    const gaps = this.gapsData();
    if (!gaps || gaps.items.length === 0) return null;

    const labels = gaps.items.map((item) => item.preferredTitle);

    // Collect unique raters in order of first appearance
    const raterMap = new Map<string, { displayName: string; scores: (number | null)[] }>();
    for (const item of gaps.items) {
      for (const r of item.ratings) {
        if (!raterMap.has(r.userId)) {
          raterMap.set(r.userId, { displayName: r.displayName, scores: [] });
        }
      }
    }

    // Fill scores
    for (const item of gaps.items) {
      const ratingByUser = new Map(item.ratings.map((r) => [r.userId, r.ratingScore]));
      for (const [userId, entry] of raterMap) {
        entry.scores.push(ratingByUser.get(userId) ?? null);
      }
    }

    const colors = [
      'var(--bloom-pink-500)',
      'var(--bloom-blue-500)',
      'var(--bloom-lime-500)',
      'var(--bloom-lilac-500)',
    ];

    const datasets = [...raterMap.values()].map((entry, i) => ({
      label: entry.displayName,
      data: entry.scores,
      backgroundColor: colors[i % colors.length],
      borderRadius: 4,
    }));

    return { labels, datasets };
  });

  readonly chartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: { min: 0, max: 10, ticks: { stepSize: 2 } },
    },
    plugins: {
      legend: { position: 'bottom' },
    },
  };

  ngOnInit(): void {
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    this.isLoading.set(true);
    this.compatError.set('');
    this.gapsError.set('');
    this.statsError.set('');

    forkJoin({
      compat: this.watchSpaceService.getCompatibility(this.spaceId).pipe(
        catchError(() => {
          this.compatError.set('Failed to load compatibility data.');
          return of(null);
        }),
      ),
      gaps: this.watchSpaceService.getRatingGaps(this.spaceId).pipe(
        catchError(() => {
          this.gapsError.set('Failed to load rating gaps.');
          return of(null);
        }),
      ),
      stats: this.watchSpaceService.getSharedStats(this.spaceId).pipe(
        catchError(() => {
          this.statsError.set('Failed to load shared stats.');
          return of(null);
        }),
      ),
    }).subscribe(({ compat, gaps, stats }) => {
      this.compatData.set(compat);
      this.gapsData.set(gaps);
      this.statsData.set(stats);
      this.isLoading.set(false);
    });
  }

  retryCompat(): void {
    this.compatError.set('');
    this.watchSpaceService.getCompatibility(this.spaceId).subscribe({
      next: (data) => this.compatData.set(data),
      error: () => this.compatError.set('Failed to load compatibility data.'),
    });
  }

  retryGaps(): void {
    this.gapsError.set('');
    this.watchSpaceService.getRatingGaps(this.spaceId).subscribe({
      next: (data) => this.gapsData.set(data),
      error: () => this.gapsError.set('Failed to load rating gaps.'),
    });
  }

  retryStats(): void {
    this.statsError.set('');
    this.watchSpaceService.getSharedStats(this.spaceId).subscribe({
      next: (data) => this.statsData.set(data),
      error: () => this.statsError.set('Failed to load shared stats.'),
    });
  }

  scoreBarWidth(score: number): number {
    return Math.min(100, (score / 10) * 100);
  }

  navigateBack(): void {
    this.router.navigate(['/watch-spaces', this.spaceId]);
  }
}
