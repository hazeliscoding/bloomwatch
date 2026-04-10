import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml, Title } from '@angular/platform-browser';
import { Subject } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomBadgeComponent, BloomBadgeColor } from '../../shared/ui/badge/bloom-badge';
import { BloomAvatarComponent } from '../../shared/ui/avatar/bloom-avatar';
import { WatchSpaceService } from './watch-space.service';
import {
  AnimeTag,
  MemberDetail,
  ParticipantDetail,
  WatchSpaceAnimeDetail,
} from './watch-space.model';
import { AuthService } from '../../core/auth/auth.service';

const STATUS_OPTIONS = ['Backlog', 'Watching', 'Finished', 'Paused', 'Dropped'];

const STATUS_BADGE_COLORS: Record<string, BloomBadgeColor> = {
  Backlog: 'blue',
  Watching: 'lilac',
  Finished: 'green',
  Paused: 'yellow',
  Dropped: 'neutral',
};

const AIRING_STATUS_COLORS: Record<string, BloomBadgeColor> = {
  RELEASING: 'green',
  FINISHED: 'neutral',
  NOT_YET_RELEASED: 'blue',
  CANCELLED: 'yellow',
  HIATUS: 'yellow',
};

const AIRING_STATUS_LABELS: Record<string, string> = {
  RELEASING: 'Airing',
  FINISHED: 'Finished Airing',
  NOT_YET_RELEASED: 'Not Yet Aired',
  CANCELLED: 'Cancelled',
  HIATUS: 'On Hiatus',
};

@Component({
  selector: 'app-anime-detail',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    BloomCardComponent,
    BloomButtonComponent,
    BloomBadgeComponent,
    BloomAvatarComponent,
  ],
  styleUrl: './anime-detail.scss',
  template: `
    <!-- Back Navigation -->
    <nav class="anime-detail__back">
      <a class="anime-detail__back-link" [routerLink]="['/watch-spaces', watchSpaceId, 'manage']">
        <span aria-hidden="true">&larr;</span> Back to Anime List
      </a>
    </nav>

    <!-- Loading -->
    @if (isLoading()) {
      <div class="anime-detail__loading" role="status">
        <div class="anime-detail__spinner">
          <span class="anime-detail__spinner-dot"></span>
          <span class="anime-detail__spinner-dot"></span>
          <span class="anime-detail__spinner-dot"></span>
        </div>
        <p>Loading anime details...</p>
      </div>
    }

    <!-- Error -->
    @if (!isLoading() && loadError()) {
      <div class="anime-detail__error" role="alert">
        <p>{{ loadError() }}</p>
        <bloom-button variant="ghost" (clicked)="loadDetail()">Retry</bloom-button>
      </div>
    }

    <!-- Content -->
    @if (!isLoading() && !loadError() && anime()) {
      <!-- Hero Section (inside a card) -->
      <bloom-card class="anime-detail__hero-card">
        <section class="anime-detail__hero">
          <div class="anime-detail__hero-cover">
            @if (anime()!.coverImageUrlSnapshot) {
              <img
                [src]="anime()!.coverImageUrlSnapshot"
                [alt]="anime()!.preferredTitle"
                (error)="onCoverError($event)"
              />
            } @else {
              <div class="anime-detail__hero-cover-placeholder"></div>
            }
          </div>
          <div class="anime-detail__hero-info">
            <h1 class="anime-detail__title bloom-font-display">{{ anime()!.preferredTitle }}</h1>
            <div class="anime-detail__meta-line">
              {{ metaLine() }}
              @if (anime()!.airingStatus) {
                <bloom-badge
                  [color]="airingStatusColor(anime()!.airingStatus!)"
                  size="sm"
                  class="anime-detail__airing-badge"
                >{{ airingStatusLabel(anime()!.airingStatus!) }}</bloom-badge>
              }
            </div>
            @if (anime()!.genres && anime()!.genres!.length > 0) {
              <div class="anime-detail__genres">
                @for (genre of anime()!.genres!; track genre) {
                  <bloom-badge [color]="genreBadgeColor($index)">{{ genre }}</bloom-badge>
                }
              </div>
            }
            <div class="anime-detail__anilist-stats">
              @if (anime()!.averageScore != null) {
                <span>AniList Score: <strong>{{ anime()!.averageScore }}</strong></span>
              }
              @if (anime()!.popularity != null) {
                <span>Popularity: <strong>#{{ anime()!.popularity }}</strong></span>
              }
            </div>
            @if (displayTags().length > 0) {
              <div class="anime-detail__tags">
                @for (tag of displayTags(); track tag.name) {
                  @if (tag.isMediaSpoiler) {
                    <bloom-badge
                      color="neutral"
                      size="sm"
                      class="anime-detail__tag"
                      [class.anime-detail__tag--spoiler]="!revealedSpoilers().has(tag.name)"
                      [class.anime-detail__tag--revealed]="revealedSpoilers().has(tag.name)"
                      (click)="revealSpoilerTag(tag.name)"
                      [attr.role]="revealedSpoilers().has(tag.name) ? undefined : 'button'"
                      [attr.aria-label]="revealedSpoilers().has(tag.name) ? undefined : 'Reveal spoiler tag'"
                    >{{ tag.name }}</bloom-badge>
                  } @else {
                    <bloom-badge color="neutral" size="sm" class="anime-detail__tag">{{ tag.name }}</bloom-badge>
                  }
                }
              </div>
            }
            @if (anime()!.description) {
              <div class="anime-detail__description" [innerHTML]="sanitizedDescription()"></div>
            }
            @if (anilistUrl()) {
              <a class="anime-detail__anilist-link" [href]="anilistUrl()" target="_blank" rel="noopener noreferrer">
                View on AniList &rarr;
              </a>
            }
          </div>
        </section>
      </bloom-card>

      <!-- Shared Status & Progress -->
      <bloom-card class="anime-detail__status-card">
        <h2 class="anime-detail__section-title bloom-font-display" bloomCardHeader>Shared Status &amp; Progress</h2>
        <div class="anime-detail__shared">
          <div class="anime-detail__shared-controls">
            <div class="anime-detail__shared-field">
              <span class="anime-detail__field-label">Status:</span>
              <select
                class="anime-detail__status-select"
                [ngModel]="anime()!.sharedStatus"
                (ngModelChange)="onSharedStatusChange($event)"
                name="sharedStatus"
              >
                @for (s of statusOptions; track s) {
                  <option [value]="s">{{ s }}</option>
                }
              </select>
            </div>
            <div class="anime-detail__shared-field">
              <span class="anime-detail__field-label">Episodes:</span>
              <div class="anime-detail__ep-stepper">
                <button
                  class="anime-detail__ep-stepper-btn"
                  type="button"
                  (click)="decrementSharedEpisode()"
                  [disabled]="anime()!.sharedEpisodesWatched <= 0"
                  aria-label="Decrease episode"
                >&lsaquo;</button>
                <span class="anime-detail__ep-stepper-value">{{ anime()!.sharedEpisodesWatched }}</span>
                <button
                  class="anime-detail__ep-stepper-btn"
                  type="button"
                  (click)="incrementSharedEpisode()"
                  [disabled]="anime()!.episodeCountSnapshot != null && anime()!.sharedEpisodesWatched >= anime()!.episodeCountSnapshot!"
                  aria-label="Increase episode"
                >&rsaquo;</button>
              </div>
              <span class="anime-detail__ep-total">/ {{ anime()!.episodeCountSnapshot ?? '?' }}</span>
            </div>
          </div>
          <div class="anime-detail__progress-bar">
            <div
              class="anime-detail__progress-fill"
              [style.width.%]="progressPercent()"
            ></div>
          </div>
          @if (sharedError()) {
            <p class="anime-detail__inline-error" role="alert">{{ sharedError() }}</p>
          }
          @if (anime()!.mood || anime()!.vibe || anime()!.pitch) {
            <div class="anime-detail__mood-tags">
              @if (anime()!.mood) {
                <span class="anime-detail__mood-tag">
                  Mood: {{ anime()!.mood }} <span class="anime-detail__mood-edit" aria-label="Edit mood">&#9998;</span>
                </span>
              }
              @if (anime()!.vibe) {
                <span class="anime-detail__mood-tag">
                  Vibe: {{ anime()!.vibe }} <span class="anime-detail__mood-edit" aria-label="Edit vibe">&#9998;</span>
                </span>
              }
              @if (anime()!.pitch) {
                <span class="anime-detail__mood-tag">
                  Pitch: &ldquo;{{ anime()!.pitch }}&rdquo; <span class="anime-detail__mood-edit" aria-label="Edit pitch">&#9998;</span>
                </span>
              }
            </div>
          }
        </div>
      </bloom-card>

      <!-- Participants -->
      <h2 class="anime-detail__section-heading bloom-font-display">Participants</h2>
      <div class="anime-detail__participants">
        <!-- Self card (editable) -->
        @if (myParticipant(); as me) {
          <div class="anime-detail__participant-card anime-detail__participant-card--self">
            <div class="anime-detail__participant-header">
              <bloom-avatar size="sm" [name]="resolveDisplayName(me.userId)"></bloom-avatar>
              <span class="anime-detail__participant-name">You ({{ resolveDisplayName(me.userId) }})</span>
              <bloom-badge color="pink" size="sm" class="anime-detail__participant-label">Your progress</bloom-badge>
            </div>

            <div class="anime-detail__participant-controls">
              <div class="anime-detail__participant-field">
                <span class="anime-detail__field-label">Status:</span>
                <select
                  class="anime-detail__status-select anime-detail__status-select--sm"
                  [(ngModel)]="progressStatus"
                  name="myStatus"
                  (ngModelChange)="submitProgress()"
                >
                  @for (s of statusOptions; track s) {
                    <option [value]="s">{{ s }}</option>
                  }
                </select>
              </div>
              <div class="anime-detail__participant-field">
                <span class="anime-detail__field-label">Eps:</span>
                <div class="anime-detail__ep-stepper anime-detail__ep-stepper--sm">
                  <button
                    class="anime-detail__ep-stepper-btn anime-detail__ep-stepper-btn--sm"
                    type="button"
                    (click)="decrementMyEpisode()"
                    [disabled]="progressEpisodes <= 0"
                    aria-label="Decrease episode"
                  >&lsaquo;</button>
                  <span class="anime-detail__ep-stepper-value">{{ progressEpisodes }}</span>
                  <button
                    class="anime-detail__ep-stepper-btn anime-detail__ep-stepper-btn--sm"
                    type="button"
                    (click)="incrementMyEpisode()"
                    [disabled]="anime()!.episodeCountSnapshot != null && progressEpisodes >= anime()!.episodeCountSnapshot!"
                    aria-label="Increase episode"
                  >&rsaquo;</button>
                </div>
                <span class="anime-detail__ep-total">/ {{ anime()!.episodeCountSnapshot ?? '?' }}</span>
              </div>
            </div>

            <div class="anime-detail__progress-bar anime-detail__progress-bar--compact">
              <div class="anime-detail__progress-fill" [style.width.%]="myProgressPercent()"></div>
            </div>

            @if (progressError()) {
              <p class="anime-detail__form-error" role="alert">{{ progressError() }}</p>
            }

            <div class="anime-detail__participant-rating">
              <span class="anime-detail__field-label">My Rating:</span>
              <div class="anime-detail__rating-display">
                <span class="anime-detail__stars" aria-hidden="true">{{ starsDisplay(ratingScore) }}</span>
                <span class="anime-detail__rating-score">{{ ratingScore }}</span>
                <span class="anime-detail__rating-max">/ 10</span>
              </div>
              <input
                type="range"
                class="anime-detail__rating-slider"
                [ngModel]="ratingScore * 2"
                (ngModelChange)="onRatingSliderChange($event)"
                name="myRating"
                min="1"
                max="20"
                step="1"
                [attr.title]="'Rating: ' + ratingScore"
              />
              <span class="anime-detail__rating-hint">Drag to rate (0.5 increments)</span>
            </div>

            @if (ratingError()) {
              <p class="anime-detail__form-error" role="alert">{{ ratingError() }}</p>
            }

            <div class="anime-detail__participant-notes-field">
              <span class="anime-detail__field-label">Notes:</span>
              <input
                type="text"
                class="anime-detail__notes-input"
                [(ngModel)]="ratingNotes"
                name="myNotes"
                (blur)="submitRating()"
                placeholder="Share your thoughts..."
              />
            </div>
          </div>
        }

        <!-- Other participants (read-only) -->
        @for (p of otherParticipants(); track p.userId) {
          <div class="anime-detail__participant-card">
            <div class="anime-detail__participant-header">
              <bloom-avatar size="sm" [name]="resolveDisplayName(p.userId)"></bloom-avatar>
              <span class="anime-detail__participant-name">{{ resolveDisplayName(p.userId) }}</span>
            </div>
            <div class="anime-detail__participant-readonly">
              <span class="anime-detail__participant-stat">
                <span class="anime-detail__field-label-muted">Status:</span>
                <bloom-badge [color]="statusBadgeColor(p.individualStatus)" size="sm">{{ p.individualStatus }}</bloom-badge>
              </span>
              <span class="anime-detail__participant-stat">
                <span class="anime-detail__field-label-muted">Eps:</span>
                <strong>{{ p.episodesWatched }}</strong> / {{ anime()!.episodeCountSnapshot ?? '?' }}
              </span>
            </div>
            <div class="anime-detail__progress-bar anime-detail__progress-bar--compact">
              <div class="anime-detail__progress-fill" [style.width.%]="participantProgress(p)"></div>
            </div>
            @if (p.ratingScore != null) {
              <div class="anime-detail__rating-display">
                <span class="anime-detail__stars" aria-hidden="true">{{ starsDisplay(p.ratingScore!) }}</span>
                <span class="anime-detail__rating-score">{{ p.ratingScore }}</span>
                <span class="anime-detail__rating-max">/ 10</span>
              </div>
            }
            @if (p.ratingNotes) {
              <p class="anime-detail__participant-notes">{{ p.ratingNotes }}</p>
            }
          </div>
        }
      </div>
    }
  `,
})
export class AnimeDetail implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly watchSpaceService = inject(WatchSpaceService);
  private readonly authService = inject(AuthService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly titleService = inject(Title);

  readonly statusOptions = STATUS_OPTIONS;
  private readonly GENRE_BADGE_COLORS: BloomBadgeColor[] = ['lilac', 'blue', 'pink', 'green', 'yellow', 'neutral'];

  // Shared status/episode error + switchMap subject
  readonly sharedError = signal('');
  private sharedErrorTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly sharedEpisode$ = new Subject<number>();
  private readonly destroy$ = new Subject<void>();

  // Data signals
  readonly anime = signal<WatchSpaceAnimeDetail | null>(null);
  readonly members = signal<MemberDetail[]>([]);
  readonly isLoading = signal(true);
  readonly loadError = signal('');

  // Route params (public so template routerLink can reference them)
  watchSpaceId = '';
  private animeId = '';

  // Computed
  readonly currentUserId = computed(() => this.authService.userId());

  readonly metaLine = computed(() => {
    const a = this.anime();
    if (!a) return '';
    const parts: string[] = [];
    if (a.format) parts.push(a.format);
    if (a.season && a.seasonYear) {
      const season = a.season.charAt(0).toUpperCase() + a.season.slice(1).toLowerCase();
      parts.push(`${season} ${a.seasonYear}`);
    }
    if (a.episodeCountSnapshot != null) parts.push(`${a.episodeCountSnapshot} Episodes`);
    return parts.join(' \u00B7 ');
  });

  readonly progressPercent = computed(() => {
    const a = this.anime();
    if (!a || !a.episodeCountSnapshot || a.episodeCountSnapshot === 0) return 0;
    return Math.min(100, (a.sharedEpisodesWatched / a.episodeCountSnapshot) * 100);
  });

  readonly myParticipant = computed<ParticipantDetail | null>(() => {
    const a = this.anime();
    const uid = this.currentUserId();
    if (!a || !uid) return null;
    return a.participants.find((p) => p.userId === uid) ?? null;
  });

  readonly otherParticipants = computed<ParticipantDetail[]>(() => {
    const a = this.anime();
    const uid = this.currentUserId();
    if (!a) return [];
    return a.participants.filter((p) => p.userId !== uid);
  });

  readonly myProgressPercent = computed(() => {
    const a = this.anime();
    if (!a || !a.episodeCountSnapshot || a.episodeCountSnapshot === 0) return 0;
    return Math.min(100, (this.progressEpisodes / a.episodeCountSnapshot) * 100);
  });

  readonly revealedSpoilers = signal<Set<string>>(new Set());

  readonly displayTags = computed<AnimeTag[]>(() => {
    const a = this.anime();
    if (!a?.tags) return [];
    return [...a.tags].sort((x, y) => y.rank - x.rank).slice(0, 15);
  });

  readonly sanitizedDescription = computed<SafeHtml>(() => {
    const a = this.anime();
    if (!a?.description) return '';
    return this.sanitizer.bypassSecurityTrustHtml(a.description);
  });

  readonly anilistUrl = computed<string | null>(() => {
    const a = this.anime();
    if (!a) return null;
    if (a.siteUrl) return a.siteUrl;
    return `https://anilist.co/anime/${a.anilistMediaId}`;
  });

  // Progress form (inline in self card)
  progressStatus = 'Backlog';
  progressEpisodes = 0;
  readonly isSubmittingProgress = signal(false);
  readonly progressError = signal('');

  // Rating (inline in self card)
  ratingScore = 5;
  ratingNotes = '';
  readonly isSubmittingRating = signal(false);
  readonly ratingError = signal('');

  ngOnInit(): void {
    this.watchSpaceId = this.route.snapshot.paramMap.get('id') ?? '';
    this.animeId = this.route.snapshot.paramMap.get('animeId') ?? '';
    this.loadDetail();

    // switchMap: only the latest shared episode value is persisted
    this.sharedEpisode$
      .pipe(
        switchMap((episodes) =>
          this.watchSpaceService.updateSharedAnime(this.watchSpaceId, this.animeId, {
            sharedEpisodesWatched: episodes,
          }),
        ),
        takeUntil(this.destroy$),
      )
      .subscribe({
        next: (updated) => {
          this.anime.set({ ...this.anime()!, sharedEpisodesWatched: updated.sharedEpisodesWatched });
        },
        error: () => {
          this.refreshDetail();
          this.showSharedError('Failed to update episodes. Reverted.');
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.sharedErrorTimer) clearTimeout(this.sharedErrorTimer);
  }

  loadDetail(): void {
    if (!this.watchSpaceId || !this.animeId) return;

    this.isLoading.set(true);
    this.loadError.set('');

    this.watchSpaceService.getAnimeDetail(this.watchSpaceId, this.animeId).subscribe({
      next: (detail) => {
        this.anime.set(detail);
        this.isLoading.set(false);
        this.prefillForms(detail);
        this.titleService.setTitle(`${detail.preferredTitle} · BloomWatch`);
      },
      error: () => {
        this.isLoading.set(false);
        this.loadError.set('Failed to load anime details. Please try again.');
      },
    });

    this.watchSpaceService.getWatchSpaceById(this.watchSpaceId).subscribe({
      next: (space) => this.members.set(space.members),
    });
  }

  onCoverError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
    const placeholder = img.parentElement?.querySelector('.anime-detail__hero-cover-placeholder');
    if (!placeholder) {
      const div = document.createElement('div');
      div.className = 'anime-detail__hero-cover-placeholder';
      img.parentElement?.appendChild(div);
    }
  }

  statusBadgeColor(status: string): BloomBadgeColor {
    return STATUS_BADGE_COLORS[status] ?? 'neutral';
  }

  genreBadgeColor(index: number): BloomBadgeColor {
    return this.GENRE_BADGE_COLORS[index % this.GENRE_BADGE_COLORS.length];
  }

  airingStatusColor(status: string): BloomBadgeColor {
    return AIRING_STATUS_COLORS[status] ?? 'neutral';
  }

  airingStatusLabel(status: string): string {
    return AIRING_STATUS_LABELS[status] ?? status;
  }

  revealSpoilerTag(tagName: string): void {
    const current = this.revealedSpoilers();
    const next = new Set(current);
    next.add(tagName);
    this.revealedSpoilers.set(next);
  }

  resolveDisplayName(userId: string): string {
    const member = this.members().find((m) => m.userId === userId);
    return member?.displayName ?? 'Unknown';
  }

  starsDisplay(score: number): string {
    const full = Math.round(score);
    return '\u2605'.repeat(full) + '\u2606'.repeat(10 - full);
  }

  participantProgress(p: ParticipantDetail): number {
    const a = this.anime();
    if (!a || !a.episodeCountSnapshot || a.episodeCountSnapshot === 0) return 0;
    return Math.min(100, (p.episodesWatched / a.episodeCountSnapshot) * 100);
  }

  // --- Shared status/episode steppers ---

  onSharedStatusChange(status: string): void {
    const a = this.anime();
    if (!a) return;
    const prev = a.sharedStatus;
    this.anime.set({ ...a, sharedStatus: status });
    this.watchSpaceService
      .updateSharedAnime(this.watchSpaceId, this.animeId, { sharedStatus: status })
      .subscribe({
        error: () => {
          this.anime.set({ ...this.anime()!, sharedStatus: prev });
          this.showSharedError('Failed to update status. Reverted.');
        },
      });
  }

  decrementSharedEpisode(): void {
    const a = this.anime();
    if (a && a.sharedEpisodesWatched > 0) {
      const newVal = a.sharedEpisodesWatched - 1;
      this.anime.set({ ...a, sharedEpisodesWatched: newVal });
      this.sharedEpisode$.next(newVal);
    }
  }

  incrementSharedEpisode(): void {
    const a = this.anime();
    if (a && (a.episodeCountSnapshot == null || a.sharedEpisodesWatched < a.episodeCountSnapshot)) {
      const newVal = a.sharedEpisodesWatched + 1;
      this.anime.set({ ...a, sharedEpisodesWatched: newVal });
      this.sharedEpisode$.next(newVal);
    }
  }

  private showSharedError(msg: string): void {
    this.sharedError.set(msg);
    if (this.sharedErrorTimer) clearTimeout(this.sharedErrorTimer);
    this.sharedErrorTimer = setTimeout(() => this.sharedError.set(''), 3000);
  }

  // --- My episode steppers ---

  decrementMyEpisode(): void {
    if (this.progressEpisodes > 0) {
      this.progressEpisodes--;
      this.submitProgress();
    }
  }

  incrementMyEpisode(): void {
    const a = this.anime();
    if (a && (a.episodeCountSnapshot == null || this.progressEpisodes < a.episodeCountSnapshot)) {
      this.progressEpisodes++;
      this.submitProgress();
    }
  }

  // --- Rating slider ---

  onRatingSliderChange(rawValue: number): void {
    this.ratingScore = rawValue / 2;
  }

  // --- Progress submit ---

  submitProgress(): void {
    if (this.progressEpisodes < 0) return;
    this.isSubmittingProgress.set(true);
    this.progressError.set('');

    this.watchSpaceService
      .updateParticipantProgress(this.watchSpaceId, this.animeId, {
        individualStatus: this.progressStatus,
        episodesWatched: this.progressEpisodes,
      })
      .subscribe({
        next: () => {
          this.isSubmittingProgress.set(false);
          this.refreshDetail();
        },
        error: () => {
          this.isSubmittingProgress.set(false);
          this.progressError.set('Failed to update progress. Please try again.');
        },
      });
  }

  // --- Rating submit ---

  isValidRating(): boolean {
    return (
      this.ratingScore >= 0.5 &&
      this.ratingScore <= 10 &&
      this.ratingScore % 0.5 === 0
    );
  }

  submitRating(): void {
    if (!this.isValidRating()) return;
    this.isSubmittingRating.set(true);
    this.ratingError.set('');

    this.watchSpaceService
      .updateParticipantRating(this.watchSpaceId, this.animeId, {
        ratingScore: this.ratingScore,
        ratingNotes: this.ratingNotes || null,
      })
      .subscribe({
        next: () => {
          this.isSubmittingRating.set(false);
          this.refreshDetail();
        },
        error: () => {
          this.isSubmittingRating.set(false);
          this.ratingError.set('Failed to submit rating. Please try again.');
        },
      });
  }

  // --- Helpers ---

  private prefillForms(detail: WatchSpaceAnimeDetail): void {
    const uid = this.currentUserId();
    const myEntry = detail.participants.find((p) => p.userId === uid);
    if (myEntry) {
      this.progressStatus = myEntry.individualStatus;
      this.progressEpisodes = myEntry.episodesWatched;
      if (myEntry.ratingScore != null) {
        this.ratingScore = myEntry.ratingScore;
      }
      if (myEntry.ratingNotes != null) {
        this.ratingNotes = myEntry.ratingNotes;
      }
    }
  }

  private refreshDetail(): void {
    this.watchSpaceService.getAnimeDetail(this.watchSpaceId, this.animeId).subscribe({
      next: (detail) => {
        this.anime.set(detail);
        this.prefillForms(detail);
      },
    });
  }
}
