import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomBadgeComponent, BloomBadgeColor } from '../../shared/ui/badge/bloom-badge';
import { WatchSpaceService } from './watch-space.service';
import {
  MemberDetail,
  WatchSpaceAnimeDetail,
  WatchSessionDetail,
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

@Component({
  selector: 'app-anime-detail',
  standalone: true,
  imports: [DatePipe, FormsModule, BloomCardComponent, BloomButtonComponent, BloomBadgeComponent],
  styleUrl: './anime-detail.scss',
  template: `
    <!-- Back Navigation -->
    <nav class="anime-detail__back">
      <button class="anime-detail__back-btn" (click)="navigateBack()">
        <span aria-hidden="true">&larr;</span> Back to Watch Space
      </button>
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
      <!-- Hero Section -->
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
          <div class="anime-detail__meta">
            @if (anime()!.format) {
              <span class="anime-detail__meta-item">{{ anime()!.format }}</span>
            }
            @if (anime()!.season && anime()!.seasonYear) {
              <span class="anime-detail__meta-item">{{ anime()!.season }} {{ anime()!.seasonYear }}</span>
            }
            @if (anime()!.episodeCountSnapshot != null) {
              <span class="anime-detail__meta-item">{{ anime()!.episodeCountSnapshot }} episodes</span>
            }
          </div>
        </div>
      </section>

      <!-- Shared Tracking State -->
      <section class="anime-detail__section">
        <h2 class="anime-detail__section-title bloom-font-display">Shared Status</h2>
        <bloom-card>
          <div class="anime-detail__shared">
            <div class="anime-detail__shared-status">
              <bloom-badge [color]="statusBadgeColor(anime()!.sharedStatus)">
                {{ anime()!.sharedStatus }}
              </bloom-badge>
              <span class="anime-detail__shared-progress">
                {{ formatProgress(anime()!.sharedEpisodesWatched, anime()!.episodeCountSnapshot) }}
              </span>
            </div>
            <div class="anime-detail__progress-bar">
              <div
                class="anime-detail__progress-fill"
                [style.width.%]="progressPercent()"
              ></div>
            </div>
            @if (anime()!.mood || anime()!.vibe || anime()!.pitch) {
              <div class="anime-detail__shared-tags">
                @if (anime()!.mood) {
                  <span class="anime-detail__tag">
                    <span class="anime-detail__tag-label">Mood</span>
                    {{ anime()!.mood }}
                  </span>
                }
                @if (anime()!.vibe) {
                  <span class="anime-detail__tag">
                    <span class="anime-detail__tag-label">Vibe</span>
                    {{ anime()!.vibe }}
                  </span>
                }
                @if (anime()!.pitch) {
                  <span class="anime-detail__tag">
                    <span class="anime-detail__tag-label">Pitch</span>
                    {{ anime()!.pitch }}
                  </span>
                }
              </div>
            }
          </div>
        </bloom-card>
      </section>

      <!-- Action Forms -->
      <section class="anime-detail__section">
        <h2 class="anime-detail__section-title bloom-font-display">Your Actions</h2>

        <!-- Update Progress -->
        <bloom-card class="anime-detail__action-card">
          <button class="anime-detail__action-toggle" (click)="toggleProgressForm()">
            <span>Update Progress</span>
            <span class="anime-detail__action-chevron" [class.anime-detail__action-chevron--open]="showProgressForm()">&#9662;</span>
          </button>
          @if (showProgressForm()) {
            <form class="anime-detail__form" (ngSubmit)="submitProgress()">
              <div class="anime-detail__form-row">
                <label class="anime-detail__form-label" for="progress-status">Status</label>
                <select
                  id="progress-status"
                  class="anime-detail__select"
                  [(ngModel)]="progressStatus"
                  name="progressStatus"
                >
                  @for (s of statusOptions; track s) {
                    <option [value]="s">{{ s }}</option>
                  }
                </select>
              </div>
              <div class="anime-detail__form-row">
                <label class="anime-detail__form-label" for="progress-episodes">Episodes Watched</label>
                <input
                  id="progress-episodes"
                  type="number"
                  class="anime-detail__number-input"
                  [(ngModel)]="progressEpisodes"
                  name="progressEpisodes"
                  min="0"
                  [max]="anime()!.episodeCountSnapshot"
                />
              </div>
              @if (progressError()) {
                <p class="anime-detail__form-error" role="alert">{{ progressError() }}</p>
              }
              <div class="anime-detail__form-actions">
                <bloom-button type="submit" variant="primary" size="sm" [loading]="isSubmittingProgress()" [disabled]="progressEpisodes < 0">
                  Save Progress
                </bloom-button>
              </div>
            </form>
          }
        </bloom-card>

        <!-- Submit Rating -->
        <bloom-card class="anime-detail__action-card">
          <button class="anime-detail__action-toggle" (click)="toggleRatingForm()">
            <span>Submit Rating</span>
            <span class="anime-detail__action-chevron" [class.anime-detail__action-chevron--open]="showRatingForm()">&#9662;</span>
          </button>
          @if (showRatingForm()) {
            <form class="anime-detail__form" (ngSubmit)="submitRating()">
              <div class="anime-detail__form-row">
                <label class="anime-detail__form-label" for="rating-score">Score (0.5 – 10.0)</label>
                <input
                  id="rating-score"
                  type="number"
                  class="anime-detail__number-input"
                  [(ngModel)]="ratingScore"
                  name="ratingScore"
                  min="0.5"
                  max="10"
                  step="0.5"
                />
              </div>
              <div class="anime-detail__form-row">
                <label class="anime-detail__form-label" for="rating-notes">Notes (optional)</label>
                <textarea
                  id="rating-notes"
                  class="anime-detail__textarea"
                  [(ngModel)]="ratingNotes"
                  name="ratingNotes"
                  maxlength="1000"
                  rows="3"
                  placeholder="Share your thoughts..."
                ></textarea>
                <span class="anime-detail__char-count">{{ ratingNotes.length }} / 1000</span>
              </div>
              @if (ratingError()) {
                <p class="anime-detail__form-error" role="alert">{{ ratingError() }}</p>
              }
              <div class="anime-detail__form-actions">
                <bloom-button
                  type="submit"
                  variant="primary"
                  size="sm"
                  [loading]="isSubmittingRating()"
                  [disabled]="!isValidRating()"
                >
                  Save Rating
                </bloom-button>
              </div>
            </form>
          }
        </bloom-card>

        <!-- Record Watch Session -->
        <bloom-card class="anime-detail__action-card">
          <button class="anime-detail__action-toggle" (click)="toggleSessionForm()">
            <span>Record Watch Session</span>
            <span class="anime-detail__action-chevron" [class.anime-detail__action-chevron--open]="showSessionForm()">&#9662;</span>
          </button>
          @if (showSessionForm()) {
            <form class="anime-detail__form" (ngSubmit)="submitSession()">
              <div class="anime-detail__form-row">
                <label class="anime-detail__form-label" for="session-date">Date</label>
                <input
                  id="session-date"
                  type="date"
                  class="anime-detail__date-input"
                  [(ngModel)]="sessionDate"
                  name="sessionDate"
                />
              </div>
              <div class="anime-detail__form-row-inline">
                <div class="anime-detail__form-row">
                  <label class="anime-detail__form-label" for="session-start">Start Episode</label>
                  <input
                    id="session-start"
                    type="number"
                    class="anime-detail__number-input"
                    [(ngModel)]="sessionStartEp"
                    name="sessionStartEp"
                    min="1"
                  />
                </div>
                <div class="anime-detail__form-row">
                  <label class="anime-detail__form-label" for="session-end">End Episode</label>
                  <input
                    id="session-end"
                    type="number"
                    class="anime-detail__number-input"
                    [(ngModel)]="sessionEndEp"
                    name="sessionEndEp"
                    min="1"
                  />
                </div>
              </div>
              <div class="anime-detail__form-row">
                <label class="anime-detail__form-label" for="session-notes">Notes (optional)</label>
                <textarea
                  id="session-notes"
                  class="anime-detail__textarea"
                  [(ngModel)]="sessionNotes"
                  name="sessionNotes"
                  rows="2"
                  placeholder="What happened this session?"
                ></textarea>
              </div>
              @if (sessionError()) {
                <p class="anime-detail__form-error" role="alert">{{ sessionError() }}</p>
              }
              <div class="anime-detail__form-actions">
                <bloom-button
                  type="submit"
                  variant="primary"
                  size="sm"
                  [loading]="isSubmittingSession()"
                  [disabled]="!isValidSession()"
                >
                  Record Session
                </bloom-button>
              </div>
            </form>
          }
        </bloom-card>
      </section>

      <!-- Participants -->
      <section class="anime-detail__section">
        <h2 class="anime-detail__section-title bloom-font-display">Participants</h2>
        <div class="anime-detail__participants">
          @for (p of anime()!.participants; track p.userId) {
            <bloom-card class="anime-detail__participant-card">
              <div class="anime-detail__participant">
                <div class="anime-detail__participant-header">
                  <span class="anime-detail__participant-name">{{ resolveDisplayName(p.userId) }}</span>
                  <bloom-badge [color]="statusBadgeColor(p.individualStatus)" size="sm">
                    {{ p.individualStatus }}
                  </bloom-badge>
                </div>
                <div class="anime-detail__participant-stats">
                  <span class="anime-detail__participant-ep">
                    {{ formatProgress(p.episodesWatched, anime()!.episodeCountSnapshot) }}
                  </span>
                  <span class="anime-detail__participant-rating">
                    @if (p.ratingScore != null) {
                      <span class="anime-detail__rating-score">{{ p.ratingScore }} / 10</span>
                      <span class="anime-detail__rating-bar">
                        <span class="anime-detail__rating-bar-fill" [style.width.%]="p.ratingScore * 10"></span>
                      </span>
                    } @else {
                      <span class="anime-detail__rating-none">No rating</span>
                    }
                  </span>
                </div>
                @if (p.ratingNotes) {
                  <p class="anime-detail__participant-notes">{{ p.ratingNotes }}</p>
                }
              </div>
            </bloom-card>
          }
        </div>
      </section>

      <!-- Watch Sessions -->
      <section class="anime-detail__section">
        <h2 class="anime-detail__section-title bloom-font-display">Watch Sessions</h2>
        @if (sortedSessions().length === 0) {
          <p class="anime-detail__empty">No watch sessions recorded yet</p>
        } @else {
          <div class="anime-detail__sessions">
            @for (session of sortedSessions(); track session.watchSessionId) {
              <bloom-card class="anime-detail__session-card">
                <div class="anime-detail__session">
                  <div class="anime-detail__session-header">
                    <span class="anime-detail__session-date">{{ session.sessionDateUtc | date:'mediumDate' }}</span>
                    <span class="anime-detail__session-eps">
                      @if (session.startEpisode === session.endEpisode) {
                        Ep {{ session.startEpisode }}
                      } @else {
                        Ep {{ session.startEpisode }}–{{ session.endEpisode }}
                      }
                    </span>
                  </div>
                  <span class="anime-detail__session-by">{{ resolveDisplayName(session.createdByUserId) }}</span>
                  @if (session.notes) {
                    <p class="anime-detail__session-notes">{{ session.notes }}</p>
                  }
                </div>
              </bloom-card>
            }
          </div>
        }
      </section>
    }
  `,
})
export class AnimeDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly watchSpaceService = inject(WatchSpaceService);
  private readonly authService = inject(AuthService);

  readonly statusOptions = STATUS_OPTIONS;

  // Data signals
  readonly anime = signal<WatchSpaceAnimeDetail | null>(null);
  readonly members = signal<MemberDetail[]>([]);
  readonly isLoading = signal(true);
  readonly loadError = signal('');

  // Route params
  private watchSpaceId = '';
  private animeId = '';

  // Computed
  readonly currentUserId = computed(() => this.authService.userId());

  readonly progressPercent = computed(() => {
    const a = this.anime();
    if (!a || !a.episodeCountSnapshot || a.episodeCountSnapshot === 0) return 0;
    return Math.min(100, (a.sharedEpisodesWatched / a.episodeCountSnapshot) * 100);
  });

  readonly sortedSessions = computed<WatchSessionDetail[]>(() => {
    const a = this.anime();
    if (!a) return [];
    return [...a.watchSessions].sort(
      (x, y) => new Date(y.sessionDateUtc).getTime() - new Date(x.sessionDateUtc).getTime(),
    );
  });

  // Progress form
  readonly showProgressForm = signal(false);
  progressStatus = 'Backlog';
  progressEpisodes = 0;
  readonly isSubmittingProgress = signal(false);
  readonly progressError = signal('');

  // Rating form
  readonly showRatingForm = signal(false);
  ratingScore = 5;
  ratingNotes = '';
  readonly isSubmittingRating = signal(false);
  readonly ratingError = signal('');

  // Session form
  readonly showSessionForm = signal(false);
  sessionDate = new Date().toISOString().split('T')[0];
  sessionStartEp = 1;
  sessionEndEp = 1;
  sessionNotes = '';
  readonly isSubmittingSession = signal(false);
  readonly sessionError = signal('');

  ngOnInit(): void {
    this.watchSpaceId = this.route.snapshot.paramMap.get('id') ?? '';
    this.animeId = this.route.snapshot.paramMap.get('animeId') ?? '';
    this.loadDetail();
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

  navigateBack(): void {
    this.router.navigate(['/watch-spaces', this.watchSpaceId]);
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

  formatProgress(watched: number, total: number | null): string {
    return total != null ? `Ep ${watched} / ${total}` : `Ep ${watched}`;
  }

  resolveDisplayName(userId: string): string {
    const member = this.members().find((m) => m.userId === userId);
    return member?.displayName ?? 'Unknown';
  }

  // --- Progress form ---

  toggleProgressForm(): void {
    this.showProgressForm.update((v) => !v);
  }

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
          this.showProgressForm.set(false);
          this.refreshDetail();
        },
        error: () => {
          this.isSubmittingProgress.set(false);
          this.progressError.set('Failed to update progress. Please try again.');
        },
      });
  }

  // --- Rating form ---

  toggleRatingForm(): void {
    this.showRatingForm.update((v) => !v);
  }

  isValidRating(): boolean {
    return (
      this.ratingScore >= 0.5 &&
      this.ratingScore <= 10 &&
      this.ratingScore % 0.5 === 0 &&
      this.ratingNotes.length <= 1000
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
          this.showRatingForm.set(false);
          this.refreshDetail();
        },
        error: () => {
          this.isSubmittingRating.set(false);
          this.ratingError.set('Failed to submit rating. Please try again.');
        },
      });
  }

  // --- Session form ---

  toggleSessionForm(): void {
    this.showSessionForm.update((v) => !v);
  }

  isValidSession(): boolean {
    return (
      !!this.sessionDate &&
      this.sessionStartEp >= 1 &&
      this.sessionEndEp >= this.sessionStartEp
    );
  }

  submitSession(): void {
    if (!this.isValidSession()) return;
    this.isSubmittingSession.set(true);
    this.sessionError.set('');

    this.watchSpaceService
      .recordWatchSession(this.watchSpaceId, this.animeId, {
        sessionDateUtc: new Date(this.sessionDate).toISOString(),
        startEpisode: this.sessionStartEp,
        endEpisode: this.sessionEndEp,
        notes: this.sessionNotes || null,
      })
      .subscribe({
        next: () => {
          this.isSubmittingSession.set(false);
          this.showSessionForm.set(false);
          this.sessionNotes = '';
          this.refreshDetail();
        },
        error: () => {
          this.isSubmittingSession.set(false);
          this.sessionError.set('Failed to record session. Please try again.');
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
