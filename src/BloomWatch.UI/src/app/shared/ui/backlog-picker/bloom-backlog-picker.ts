import { Component, inject, input, OnInit, output, signal } from '@angular/core';
import { RandomPickAnimeResult, RandomPickResult } from '../../../features/watch-spaces/watch-space.model';
import { WatchSpaceService } from '../../../features/watch-spaces/watch-space.service';
import { BloomBadgeComponent } from '../badge/bloom-badge';
import { BloomButtonComponent } from '../button/bloom-button';

@Component({
  selector: 'bloom-backlog-picker',
  standalone: true,
  imports: [BloomBadgeComponent, BloomButtonComponent],
  styleUrl: './bloom-backlog-picker.scss',
  template: `
    @if (loading()) {
      <div class="picker__skeleton" aria-label="Loading random pick">
        <div class="picker__skeleton-cover"></div>
        <div class="picker__skeleton-lines">
          <div class="picker__skeleton-line picker__skeleton-line--title"></div>
          <div class="picker__skeleton-line picker__skeleton-line--ep"></div>
          <div class="picker__skeleton-line picker__skeleton-line--badges"></div>
        </div>
      </div>
    } @else if (!pick() && message()) {
      <div class="picker__empty">
        <p class="picker__empty-message">{{ message() }}</p>
      </div>
    } @else if (pick(); as p) {
      <div class="picker__card">
        <div class="picker__cover">
          @if (p.coverImageUrlSnapshot) {
            <img [src]="p.coverImageUrlSnapshot" [alt]="p.preferredTitle" loading="lazy" />
          } @else {
            <div class="picker__cover-placeholder">&#127916;</div>
          }
        </div>
        <div class="picker__info">
          <h4 class="picker__title">{{ p.preferredTitle }}</h4>
          @if (p.episodeCountSnapshot) {
            <p class="picker__meta">{{ p.episodeCountSnapshot }} episodes</p>
          }
          <div class="picker__badges">
            @if (p.mood) {
              <bloom-badge color="lilac" size="sm">Mood: {{ p.mood }}</bloom-badge>
            }
            @if (p.vibe) {
              <bloom-badge color="blue" size="sm">Vibe: {{ p.vibe }}</bloom-badge>
            }
          </div>
          <div class="picker__actions">
            <bloom-button variant="secondary" size="sm" (clicked)="reroll()">&#127922; Reroll</bloom-button>
            <bloom-button variant="ghost" size="sm" (clicked)="onViewDetails()">View Details &rarr;</bloom-button>
          </div>
        </div>
      </div>
    }
  `,
})
export class BloomBacklogPickerComponent implements OnInit {
  readonly spaceId = input.required<string>();
  readonly picked = output<string>();

  private readonly watchSpaceService = inject(WatchSpaceService);

  readonly loading = signal(true);
  readonly pick = signal<RandomPickAnimeResult | null>(null);
  readonly message = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchPick();
  }

  reroll(): void {
    this.fetchPick();
  }

  onViewDetails(): void {
    const p = this.pick();
    if (p) {
      this.picked.emit(p.watchSpaceAnimeId);
    }
  }

  private fetchPick(): void {
    this.loading.set(true);
    this.watchSpaceService.getRandomPick(this.spaceId()).subscribe({
      next: (result: RandomPickResult) => {
        this.pick.set(result.pick);
        this.message.set(result.message);
        this.loading.set(false);
      },
      error: () => {
        this.message.set('Failed to load a pick. Try again later.');
        this.pick.set(null);
        this.loading.set(false);
      },
    });
  }
}
