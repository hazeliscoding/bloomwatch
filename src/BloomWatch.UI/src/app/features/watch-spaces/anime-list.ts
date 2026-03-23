import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomBadgeComponent, BloomBadgeColor } from '../../shared/ui/badge/bloom-badge';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomAvatarComponent } from '../../shared/ui/avatar/bloom-avatar';
import { WatchSpaceService } from './watch-space.service';
import { AnimeParticipantSummary, WatchSpaceAnimeListItem } from './watch-space.model';
import { AuthService } from '../../core/auth/auth.service';

type StatusTab = 'all' | 'backlog' | 'watching' | 'finished' | 'paused' | 'dropped';

const STATUS_TABS: { value: StatusTab; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'backlog', label: 'Backlog' },
  { value: 'watching', label: 'Watching' },
  { value: 'finished', label: 'Finished' },
  { value: 'paused', label: 'Paused' },
  { value: 'dropped', label: 'Dropped' },
];

const EMPTY_MESSAGES: Record<StatusTab, string> = {
  all: 'No anime yet — add one to get started!',
  backlog: 'Nothing in Backlog yet',
  watching: 'Nothing currently Watching',
  finished: 'Nothing Finished yet',
  paused: 'Nothing Paused',
  dropped: 'Nothing Dropped',
};

const STATUS_BADGE_COLORS: Record<string, BloomBadgeColor> = {
  Backlog: 'blue',
  Watching: 'lilac',
  Finished: 'green',
  Paused: 'yellow',
  Dropped: 'neutral',
};

@Component({
  selector: 'app-anime-list',
  standalone: true,
  imports: [FormsModule, BloomCardComponent, BloomBadgeComponent, BloomButtonComponent, BloomAvatarComponent],
  styleUrl: './anime-list.scss',
  template: `
    <section class="anime-list">
      <!-- Status Tabs -->
      <nav class="anime-list__tabs" role="tablist" aria-label="Filter by status">
        @for (tab of tabs; track tab.value) {
          <button
            class="anime-list__tab"
            [class.anime-list__tab--active]="activeTab() === tab.value"
            role="tab"
            [attr.aria-selected]="activeTab() === tab.value"
            (click)="setTab(tab.value)"
          >
            {{ tab.label }}
            <span class="anime-list__tab-count">({{ countForTab(tab.value) }})</span>
          </button>
        }
      </nav>

      <!-- Loading -->
      @if (isLoading()) {
        <div class="anime-list__loading" role="status">
          <div class="anime-list__spinner">
            <span class="anime-list__spinner-dot"></span>
            <span class="anime-list__spinner-dot"></span>
            <span class="anime-list__spinner-dot"></span>
          </div>
          <p>Loading anime...</p>
        </div>
      }

      <!-- Error -->
      @if (!isLoading() && loadError()) {
        <div class="anime-list__error" role="alert">
          <p>{{ loadError() }}</p>
          <button class="anime-list__retry" (click)="refresh()">Retry</button>
        </div>
      }

      <!-- Empty State -->
      @if (!isLoading() && !loadError() && filteredList().length === 0 && animeList().length >= 0) {
        <div class="anime-list__empty">
          <p>{{ emptyMessage() }}</p>
        </div>
      }

      <!-- Anime Grid -->
      @if (!isLoading() && !loadError() && filteredList().length > 0) {
        <div class="anime-list__grid">
          @for (anime of filteredList(); track anime.watchSpaceAnimeId) {
            <bloom-card class="anime-list__card">
              <div class="anime-list__card-inner">
                <div class="anime-list__card-cover">
                  @if (anime.coverImageUrlSnapshot) {
                    <img
                      [src]="anime.coverImageUrlSnapshot"
                      [alt]="anime.preferredTitle"
                      loading="lazy"
                    />
                  } @else {
                    <div class="anime-list__card-cover-placeholder">&#127912;</div>
                  }
                </div>
                <div class="anime-list__card-info">
                  <span class="anime-list__card-title">{{ anime.preferredTitle }}</span>

                  @if (formatMetaLine(anime); as meta) {
                    <span class="anime-list__card-meta-line">{{ meta }}</span>
                  }

                  <select
                    class="anime-list__status-select"
                    [ngModel]="anime.sharedStatus"
                    (ngModelChange)="onStatusChange(anime, $event)"
                    (click)="$event.stopPropagation()"
                  >
                    @for (s of statusOptions; track s) {
                      <option [value]="s">{{ s }}</option>
                    }
                  </select>

                  <span class="anime-list__card-shared-progress">
                    Shared: {{ formatProgress(anime) }}
                  </span>

                  @if (anime.episodeCountSnapshot) {
                    <div class="anime-list__progress-bar">
                      <div
                        class="anime-list__progress-fill"
                        [style.width.%]="progressPercent(anime)"
                      ></div>
                    </div>
                  }

                  @if (anime.participants.length > 0) {
                    <div class="anime-list__card-participants">
                      @for (p of anime.participants; track p.userId) {
                        <div class="anime-list__participant-row">
                          <bloom-avatar size="xs" [name]="p.displayName" />
                          <span class="anime-list__participant-name">{{ p.displayName }}:</span>
                          <span class="anime-list__participant-ep">Ep {{ p.episodesWatched }}</span>
                          @if (p.userId === currentUserId()) {
                            <button
                              class="anime-list__plus-btn"
                              type="button"
                              title="Increment episode"
                              aria-label="Increment episode for {{ p.displayName }}"
                              [disabled]="anime.episodeCountSnapshot != null && p.episodesWatched >= anime.episodeCountSnapshot!"
                              (click)="onIncrementEpisode(anime, p); $event.stopPropagation()"
                            >+</button>
                          }
                        </div>
                      }
                    </div>
                  }

                  @if (anime.mood || anime.vibe) {
                    <div class="anime-list__card-tags">
                      @if (anime.mood) {
                        <bloom-badge color="lilac" size="sm">Mood: {{ anime.mood }}</bloom-badge>
                      }
                      @if (anime.vibe) {
                        <bloom-badge color="blue" size="sm">Vibe: {{ anime.vibe }}</bloom-badge>
                      }
                    </div>
                  }
                </div>
              </div>

              @if (cardErrors().get(anime.watchSpaceAnimeId); as err) {
                <p class="anime-list__card-error" role="alert">{{ err }}</p>
              }

              <div bloomCardFooter class="anime-list__card-footer">
                <bloom-button
                  variant="ghost"
                  size="sm"
                  (clicked)="navigateToDetail(anime.watchSpaceAnimeId)"
                >
                  View Details →
                </bloom-button>
              </div>
            </bloom-card>
          }
        </div>
      }
    </section>
  `,
})
export class AnimeListComponent implements OnInit {
  readonly watchSpaceId = input.required<string>();

  private readonly watchSpaceService = inject(WatchSpaceService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly statusOptions = ['Backlog', 'Watching', 'Finished', 'Paused', 'Dropped'];
  readonly currentUserId = computed(() => this.authService.userId());
  readonly cardErrors = signal<Map<string, string>>(new Map());
  private errorTimers = new Map<string, ReturnType<typeof setTimeout>>();

  readonly tabs = STATUS_TABS;
  readonly animeList = signal<WatchSpaceAnimeListItem[]>([]);
  readonly isLoading = signal(false);
  readonly loadError = signal('');
  readonly activeTab = signal<StatusTab>('all');

  readonly filteredList = computed(() => {
    const tab = this.activeTab();
    const list = this.animeList();
    if (tab === 'all') return list;
    return list.filter(
      (a) => a.sharedStatus.toLowerCase() === tab,
    );
  });

  readonly emptyMessage = computed(() => EMPTY_MESSAGES[this.activeTab()]);

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.isLoading.set(true);
    this.loadError.set('');
    this.watchSpaceService.listWatchSpaceAnime(this.watchSpaceId()).subscribe({
      next: (items) => {
        this.animeList.set(items);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.loadError.set('Failed to load anime list. Please try again.');
      },
    });
  }

  setTab(tab: StatusTab): void {
    this.activeTab.set(tab);
  }

  countForTab(tab: StatusTab): number {
    if (tab === 'all') return this.animeList().length;
    return this.animeList().filter(
      (a) => a.sharedStatus.toLowerCase() === tab,
    ).length;
  }

  statusBadgeColor(status: string): BloomBadgeColor {
    return STATUS_BADGE_COLORS[status] ?? 'neutral';
  }

  formatProgress(anime: WatchSpaceAnimeListItem): string {
    const watched = anime.sharedEpisodesWatched;
    const total = anime.episodeCountSnapshot;
    return total != null ? `Ep ${watched} / ${total}` : `Ep ${watched}`;
  }

  formatMetaLine(anime: WatchSpaceAnimeListItem): string {
    const parts: string[] = [];
    if (anime.formatSnapshot) parts.push(anime.formatSnapshot);
    if (anime.episodeCountSnapshot != null) parts.push(`${anime.episodeCountSnapshot} eps`);
    if (anime.seasonSnapshot && anime.seasonYearSnapshot) {
      parts.push(`${anime.seasonSnapshot} ${anime.seasonYearSnapshot}`);
    }
    return parts.join(' · ');
  }

  progressPercent(anime: WatchSpaceAnimeListItem): number {
    if (!anime.episodeCountSnapshot || anime.episodeCountSnapshot === 0) return 0;
    return Math.min(100, (anime.sharedEpisodesWatched / anime.episodeCountSnapshot) * 100);
  }

  onStatusChange(anime: WatchSpaceAnimeListItem, newStatus: string): void {
    const prev = anime.sharedStatus;
    this.updateAnimeInList(anime.watchSpaceAnimeId, { sharedStatus: newStatus });
    this.watchSpaceService
      .updateSharedAnime(this.watchSpaceId(), anime.watchSpaceAnimeId, { sharedStatus: newStatus })
      .subscribe({
        error: () => {
          this.updateAnimeInList(anime.watchSpaceAnimeId, { sharedStatus: prev });
          this.showCardError(anime.watchSpaceAnimeId, 'Failed to update status.');
        },
      });
  }

  onIncrementEpisode(anime: WatchSpaceAnimeListItem, p: { userId: string; episodesWatched: number; individualStatus: string }): void {
    if (anime.episodeCountSnapshot != null && p.episodesWatched >= anime.episodeCountSnapshot) return;
    const prevEp = p.episodesWatched;
    const newEp = prevEp + 1;
    this.updateParticipantInList(anime.watchSpaceAnimeId, p.userId, { episodesWatched: newEp });
    this.watchSpaceService
      .updateParticipantProgress(this.watchSpaceId(), anime.watchSpaceAnimeId, {
        individualStatus: p.individualStatus,
        episodesWatched: newEp,
      })
      .subscribe({
        error: () => {
          this.updateParticipantInList(anime.watchSpaceAnimeId, p.userId, { episodesWatched: prevEp });
          this.showCardError(anime.watchSpaceAnimeId, 'Failed to update episode.');
        },
      });
  }

  private updateAnimeInList(animeId: string, patch: Partial<WatchSpaceAnimeListItem>): void {
    this.animeList.update((list) =>
      list.map((a) => (a.watchSpaceAnimeId === animeId ? { ...a, ...patch } : a)),
    );
  }

  private updateParticipantInList(animeId: string, userId: string, patch: { episodesWatched: number }): void {
    this.animeList.update((list) =>
      list.map((a) => {
        if (a.watchSpaceAnimeId !== animeId) return a;
        return {
          ...a,
          participants: a.participants.map((p) =>
            p.userId === userId ? { ...p, ...patch } : p,
          ),
        };
      }),
    );
  }

  private showCardError(animeId: string, msg: string): void {
    const existing = this.errorTimers.get(animeId);
    if (existing) clearTimeout(existing);
    this.cardErrors.update((m) => new Map(m).set(animeId, msg));
    this.errorTimers.set(
      animeId,
      setTimeout(() => {
        this.cardErrors.update((m) => {
          const next = new Map(m);
          next.delete(animeId);
          return next;
        });
        this.errorTimers.delete(animeId);
      }, 3000),
    );
  }

  navigateToDetail(watchSpaceAnimeId: string): void {
    this.router.navigate(['/watch-spaces', this.watchSpaceId(), 'anime', watchSpaceAnimeId]);
  }
}
