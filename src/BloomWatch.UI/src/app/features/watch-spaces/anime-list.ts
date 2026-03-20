import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomBadgeComponent, BloomBadgeColor } from '../../shared/ui/badge/bloom-badge';
import { WatchSpaceService } from './watch-space.service';
import { WatchSpaceAnimeListItem } from './watch-space.model';

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
  imports: [BloomCardComponent, BloomBadgeComponent],
  styleUrl: './anime-list.scss',
  template: `
    <section class="anime-list">
      <h2 class="anime-list__title bloom-font-display">Anime</h2>

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
            @if (tab.value !== 'all') {
              <span class="anime-list__tab-count">{{ countForStatus(tab.value) }}</span>
            }
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
            <bloom-card
              class="anime-list__card"
              (click)="navigateToDetail(anime.watchSpaceAnimeId)"
              (keydown.enter)="navigateToDetail(anime.watchSpaceAnimeId)"
              tabindex="0"
              role="link"
            >
              <div class="anime-list__card-inner">
                <div class="anime-list__card-cover">
                  @if (anime.coverImageUrlSnapshot) {
                    <img
                      [src]="anime.coverImageUrlSnapshot"
                      [alt]="anime.preferredTitle"
                      loading="lazy"
                    />
                  } @else {
                    <div class="anime-list__card-cover-placeholder"></div>
                  }
                </div>
                <div class="anime-list__card-info">
                  <span class="anime-list__card-title">{{ anime.preferredTitle }}</span>
                  <div class="anime-list__card-meta">
                    <bloom-badge [color]="statusBadgeColor(anime.sharedStatus)" size="sm">
                      {{ anime.sharedStatus }}
                    </bloom-badge>
                    <span class="anime-list__card-progress">
                      {{ formatProgress(anime) }}
                    </span>
                  </div>
                  @if (anime.participants.length > 0) {
                    <ul class="anime-list__card-participants">
                      @for (p of anime.participants; track p.userId) {
                        <li class="anime-list__card-participant">
                          <span class="anime-list__card-participant-name">{{ p.displayName }}</span>
                          <span class="anime-list__card-participant-ep">Ep {{ p.episodesWatched }}</span>
                        </li>
                      }
                    </ul>
                  }
                </div>
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
  private readonly router = inject(Router);

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

  countForStatus(status: string): number {
    return this.animeList().filter(
      (a) => a.sharedStatus.toLowerCase() === status,
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

  navigateToDetail(watchSpaceAnimeId: string): void {
    this.router.navigate(['/watch-spaces', this.watchSpaceId(), 'anime', watchSpaceAnimeId]);
  }
}
