import { Component, inject, input, OnInit, output, signal } from '@angular/core';
import { RandomPickAnimeResult, RandomPickResult } from '../../../features/watch-spaces/watch-space.model';
import { WatchSpaceService } from '../../../features/watch-spaces/watch-space.service';

@Component({
  selector: 'bloom-backlog-picker',
  standalone: true,
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
            <div class="picker__cover-placeholder">&#127912;</div>
          }
        </div>
        <div class="picker__info">
          <h3 class="picker__title">{{ p.preferredTitle }}</h3>
          @if (p.episodeCountSnapshot) {
            <p class="picker__episodes">{{ p.episodeCountSnapshot }} episodes</p>
          }
          <div class="picker__badges">
            @if (p.mood) {
              <span class="picker__badge picker__badge--mood">{{ p.mood }}</span>
            }
            @if (p.vibe) {
              <span class="picker__badge picker__badge--vibe">{{ p.vibe }}</span>
            }
          </div>
          @if (p.pitch) {
            <p class="picker__pitch">{{ p.pitch }}</p>
          }
          <div class="picker__actions">
            <button class="picker__btn picker__btn--view" (click)="onViewDetails()">View Details</button>
            <button class="picker__btn picker__btn--reroll" (click)="reroll()">Reroll</button>
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
