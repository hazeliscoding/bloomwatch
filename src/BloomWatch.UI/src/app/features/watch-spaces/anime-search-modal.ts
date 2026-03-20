import {
  Component,
  input,
  output,
  signal,
  effect,
  inject,
  OnDestroy,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BloomModalComponent } from '../../shared/ui/modal/bloom-modal';
import { BloomInputComponent } from '../../shared/ui/input/bloom-input';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomBadgeComponent } from '../../shared/ui/badge/bloom-badge';
import { WatchSpaceService } from './watch-space.service';
import { AnimeSearchResult } from './watch-space.model';
import { Subscription, switchMap } from 'rxjs';

type AddState = 'idle' | 'adding' | 'added' | 'error';

interface SearchResultItem extends AnimeSearchResult {
  addState: AddState;
}

@Component({
  selector: 'app-anime-search-modal',
  standalone: true,
  imports: [FormsModule, BloomModalComponent, BloomInputComponent, BloomButtonComponent, BloomBadgeComponent],
  styleUrl: './anime-search-modal.scss',
  template: `
    <bloom-modal [open]="open()" width="40rem" (closed)="onModalClosed()">
      <h2 bloomModalHeader class="anime-search__title bloom-font-display">Search Anime</h2>

      <div class="anime-search__input-wrapper">
        <bloom-input
          placeholder="Search by title..."
          [ngModel]="query()"
          (valueChange)="onQueryChange($event)"
          #searchInput
        />
      </div>

      <!-- Loading -->
      @if (isLoading()) {
        <div class="anime-search__loading" role="status">
          <div class="anime-search__spinner">
            <span class="anime-search__spinner-dot"></span>
            <span class="anime-search__spinner-dot"></span>
            <span class="anime-search__spinner-dot"></span>
          </div>
          <p>Searching...</p>
        </div>
      }

      <!-- Error -->
      @if (searchError()) {
        <div class="anime-search__error" role="alert">
          <p>{{ searchError() }}</p>
          <bloom-button variant="ghost" size="sm" (clicked)="retrySearch()">
            Retry
          </bloom-button>
        </div>
      }

      <!-- Empty state -->
      @if (!isLoading() && !searchError() && hasSearched() && results().length === 0) {
        <div class="anime-search__empty">
          <p>No anime found for "{{ query() }}"</p>
        </div>
      }

      <!-- Results -->
      @if (!isLoading() && !searchError() && results().length > 0) {
        <ul class="anime-search__results">
          @for (item of results(); track item.anilistMediaId) {
            <li class="anime-search__result">
              <div class="anime-search__result-cover">
                @if (item.coverImageUrl) {
                  <img
                    [src]="item.coverImageUrl"
                    [alt]="preferredTitle(item)"
                    loading="lazy"
                  />
                } @else {
                  <div class="anime-search__result-cover-placeholder"></div>
                }
              </div>
              <div class="anime-search__result-info">
                <span class="anime-search__result-title">{{ preferredTitle(item) }}</span>
                <div class="anime-search__result-meta">
                  @if (item.format) {
                    <bloom-badge color="blue" size="sm">{{ item.format }}</bloom-badge>
                  }
                  @if (item.episodes) {
                    <span class="anime-search__result-episodes">{{ item.episodes }} ep</span>
                  }
                  @if (item.season && item.seasonYear) {
                    <span class="anime-search__result-season">{{ item.season }} {{ item.seasonYear }}</span>
                  } @else if (item.seasonYear) {
                    <span class="anime-search__result-season">{{ item.seasonYear }}</span>
                  }
                </div>
                <div class="anime-search__result-genres">
                  @for (genre of item.genres.slice(0, 3); track genre) {
                    <bloom-badge color="lilac" size="sm">{{ genre }}</bloom-badge>
                  }
                </div>
              </div>
              <div class="anime-search__result-action">
                @if (item.addState === 'added') {
                  <span class="anime-search__result-added">Added</span>
                } @else if (item.addState === 'error') {
                  <bloom-button variant="danger" size="sm" (clicked)="addAnime(item)">
                    Retry
                  </bloom-button>
                } @else {
                  <bloom-button
                    variant="accent"
                    size="sm"
                    [loading]="item.addState === 'adding'"
                    [disabled]="item.addState === 'adding'"
                    (clicked)="addAnime(item)"
                  >
                    Add
                  </bloom-button>
                }
              </div>
            </li>
          }
        </ul>
      }
    </bloom-modal>
  `,
})
export class AnimeSearchModalComponent implements OnDestroy {
  readonly open = input<boolean>(false);
  readonly watchSpaceId = input.required<string>();

  readonly closed = output<void>();
  readonly animeAdded = output<void>();

  private readonly watchSpaceService = inject(WatchSpaceService);

  readonly query = signal('');
  readonly results = signal<SearchResultItem[]>([]);
  readonly isLoading = signal(false);
  readonly searchError = signal('');
  readonly hasSearched = signal(false);

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;
  private searchSub: Subscription | null = null;
  private addedCount = 0;
  private readonly existingAniListIds = new Set<number>();

  constructor() {
    effect(() => {
      const q = this.query();
      this.scheduleSearch(q);
    });

    // Auto-focus search input when modal opens
    effect(() => {
      if (this.open()) {
        this.loadExistingAnime();
        setTimeout(() => {
          const inputEl = document.querySelector<HTMLInputElement>(
            'app-anime-search-modal bloom-input input'
          );
          inputEl?.focus();
        }, 50);
      } else {
        // Reset state when modal closes
        this.query.set('');
        this.results.set([]);
        this.isLoading.set(false);
        this.searchError.set('');
        this.hasSearched.set(false);
        this.existingAniListIds.clear();
      }
    });
  }

  ngOnDestroy(): void {
    this.clearDebounce();
    this.searchSub?.unsubscribe();
  }

  onQueryChange(value: string): void {
    this.query.set(value);
  }

  onModalClosed(): void {
    if (this.addedCount > 0) {
      this.animeAdded.emit();
    }
    this.addedCount = 0;
    this.closed.emit();
  }

  retrySearch(): void {
    this.searchError.set('');
    this.executeSearch(this.query());
  }

  addAnime(item: SearchResultItem): void {
    this.updateItemState(item.anilistMediaId, 'adding');

    this.watchSpaceService
      .ensureMediaCached(item.anilistMediaId)
      .pipe(
        switchMap(() =>
          this.watchSpaceService.addAnimeToWatchSpace(this.watchSpaceId(), {
            aniListMediaId: item.anilistMediaId,
          }),
        ),
      )
      .subscribe({
        next: () => {
          this.updateItemState(item.anilistMediaId, 'added');
          this.existingAniListIds.add(item.anilistMediaId);
          this.addedCount++;
          this.animeAdded.emit();
        },
        error: () => {
          this.updateItemState(item.anilistMediaId, 'error');
        },
      });
  }

  preferredTitle(item: AnimeSearchResult): string {
    return item.titleEnglish || item.titleRomaji || 'Unknown Title';
  }

  private scheduleSearch(query: string): void {
    this.clearDebounce();

    if (!query.trim()) {
      this.searchSub?.unsubscribe();
      this.results.set([]);
      this.isLoading.set(false);
      this.searchError.set('');
      this.hasSearched.set(false);
      return;
    }

    this.debounceTimer = setTimeout(() => {
      this.executeSearch(query);
    }, 300);
  }

  private executeSearch(query: string): void {
    if (!query.trim()) return;

    this.searchSub?.unsubscribe();
    this.isLoading.set(true);
    this.searchError.set('');

    this.searchSub = this.watchSpaceService.searchAnime(query).subscribe({
      next: (results) => {
        this.results.set(results.map((r) => ({
          ...r,
          addState: this.existingAniListIds.has(r.anilistMediaId) ? 'added' as AddState : 'idle' as AddState,
        })));
        this.isLoading.set(false);
        this.hasSearched.set(true);
      },
      error: () => {
        this.isLoading.set(false);
        this.searchError.set('Search failed. Please try again.');
        this.hasSearched.set(true);
      },
    });
  }

  private loadExistingAnime(): void {
    this.watchSpaceService.listWatchSpaceAnime(this.watchSpaceId()).subscribe({
      next: (items) => {
        for (const item of items) {
          this.existingAniListIds.add(item.anilistMediaId);
        }
      },
    });
  }

  private updateItemState(anilistMediaId: number, state: AddState): void {
    this.results.update((items) =>
      items.map((i) => (i.anilistMediaId === anilistMediaId ? { ...i, addState: state } : i))
    );
  }

  private clearDebounce(): void {
    if (this.debounceTimer !== null) {
      clearTimeout(this.debounceTimer);
      this.debounceTimer = null;
    }
  }
}
