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
import { BloomBadgeComponent, BloomBadgeColor } from '../../shared/ui/badge/bloom-badge';
import { WatchSpaceService } from './watch-space.service';
import { AnimeSearchResult } from './watch-space.model';
import { Subscription, switchMap } from 'rxjs';

type AddState = 'idle' | 'confirm' | 'adding' | 'added' | 'error';

interface SearchResultItem extends AnimeSearchResult {
  addState: AddState;
}

const GENRE_BADGE_COLORS: BloomBadgeColor[] = ['lilac', 'blue', 'pink', 'green', 'yellow', 'neutral'];

@Component({
  selector: 'app-anime-search-modal',
  standalone: true,
  imports: [FormsModule, BloomModalComponent, BloomInputComponent, BloomButtonComponent, BloomBadgeComponent],
  styleUrl: './anime-search-modal.scss',
  template: `
    <bloom-modal [open]="open()" width="36rem" (closed)="onModalClosed()">
      <h2 bloomModalHeader class="anime-search__title bloom-font-display">Search Anime</h2>

      <!-- Search Input -->
      <div class="anime-search__input-wrapper">
        <bloom-input
          placeholder="Search by title..."
          [ngModel]="query()"
          (valueChange)="onQueryChange($event)"
          hint="Results update as you type (400ms debounce)"
          #searchInput
        />
      </div>

      <!-- Loading Skeleton -->
      @if (isLoading()) {
        <div class="anime-search__skeletons" role="status" aria-label="Loading results">
          @for (i of [1, 2, 3]; track i) {
            <div class="anime-search__skeleton-row">
              <div class="anime-search__shimmer anime-search__shimmer--cover"></div>
              <div class="anime-search__skeleton-info">
                <div class="anime-search__shimmer" style="width: 70%"></div>
                <div class="anime-search__shimmer anime-search__shimmer--sm" style="width: 50%"></div>
                <div class="anime-search__shimmer anime-search__shimmer--sm" style="width: 40%"></div>
              </div>
            </div>
          }
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
          <span class="anime-search__empty-icon" aria-hidden="true">&#128270;</span>
          <p>No results for &ldquo;{{ query() }}&rdquo;. Try a different search term.</p>
        </div>
      }

      <!-- Results -->
      @if (!isLoading() && !searchError() && results().length > 0) {
        <ul class="anime-search__results">
          @for (item of results(); track item.anilistMediaId) {
            <li
              class="anime-search__result"
              [class.anime-search__result--added]="item.addState === 'added'"
            >
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
                @if (romajiSubtitle(item)) {
                  <span class="anime-search__result-subtitle">{{ romajiSubtitle(item) }}</span>
                }
                <span class="anime-search__result-meta">{{ metaLine(item) }}</span>
                @if (item.genres.length > 0) {
                  <div class="anime-search__result-genres">
                    @for (genre of item.genres.slice(0, 3); track genre) {
                      <bloom-badge [color]="genreBadgeColor($index)" size="sm">{{ genre }}</bloom-badge>
                    }
                  </div>
                }
              </div>
              <div class="anime-search__result-action">
                @if (item.addState === 'added') {
                  <bloom-badge class="anime-search__result-added" color="green" size="sm">&#10003; Added</bloom-badge>
                } @else if (item.addState === 'error') {
                  <bloom-button variant="danger" size="sm" (clicked)="startAdd(item)">
                    Retry
                  </bloom-button>
                } @else if (item.addState !== 'confirm' && item.addState !== 'adding') {
                  <bloom-button
                    variant="primary"
                    size="sm"
                    (clicked)="startAdd(item)"
                  >
                    + Add
                  </bloom-button>
                }
              </div>
            </li>
          }
        </ul>

        <!-- Add Details Panel -->
        @if (confirmingItem()) {
          <div class="anime-search__add-details">
            <div class="anime-search__add-details-title">Adding: {{ preferredTitle(confirmingItem()!) }}</div>
            <div class="anime-search__add-details-fields">
              <div class="anime-search__add-field">
                <label class="anime-search__add-label">Mood <span class="anime-search__add-optional">(optional)</span></label>
                <input
                  type="text"
                  class="anime-search__add-input"
                  [(ngModel)]="addMood"
                  name="addMood"
                  placeholder='e.g. "Cozy", "Hype", "Emotional"'
                />
              </div>
              <div class="anime-search__add-field">
                <label class="anime-search__add-label">Vibe <span class="anime-search__add-optional">(optional)</span></label>
                <input
                  type="text"
                  class="anime-search__add-input"
                  [(ngModel)]="addVibe"
                  name="addVibe"
                  placeholder='e.g. "Sunday evening vibes"'
                />
              </div>
              <div class="anime-search__add-field">
                <label class="anime-search__add-label">Pitch <span class="anime-search__add-optional">(optional)</span></label>
                <input
                  type="text"
                  class="anime-search__add-input"
                  [(ngModel)]="addPitch"
                  name="addPitch"
                  placeholder='"Why should we watch this?"'
                />
              </div>
              @if (addError()) {
                <p class="anime-search__add-error" role="alert">{{ addError() }}</p>
              }
              <div class="anime-search__add-actions">
                <bloom-button variant="ghost" size="md" (clicked)="cancelAdd()">Cancel</bloom-button>
                <bloom-button
                  variant="primary"
                  size="md"
                  [loading]="isAdding()"
                  (clicked)="confirmAdd()"
                >
                  Add to Space
                </bloom-button>
              </div>
            </div>
          </div>
        }
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

  // Add details step
  readonly confirmingItem = signal<SearchResultItem | null>(null);
  readonly isAdding = signal(false);
  readonly addError = signal('');
  addMood = '';
  addVibe = '';
  addPitch = '';

  private debounceTimer: ReturnType<typeof setTimeout> | null = null;
  private searchSub: Subscription | null = null;
  private addedCount = 0;
  private readonly existingAniListIds = new Set<number>();

  constructor() {
    effect(() => {
      const q = this.query();
      this.scheduleSearch(q);
    });

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
        this.query.set('');
        this.results.set([]);
        this.isLoading.set(false);
        this.searchError.set('');
        this.hasSearched.set(false);
        this.existingAniListIds.clear();
        this.confirmingItem.set(null);
        this.resetAddFields();
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

  preferredTitle(item: AnimeSearchResult): string {
    return item.titleEnglish || item.titleRomaji || 'Unknown Title';
  }

  romajiSubtitle(item: AnimeSearchResult): string | null {
    if (item.titleEnglish && item.titleRomaji && item.titleRomaji !== item.titleEnglish) {
      return item.titleRomaji;
    }
    return null;
  }

  metaLine(item: AnimeSearchResult): string {
    const parts: string[] = [];
    if (item.format) parts.push(item.format);
    if (item.episodes) parts.push(`${item.episodes} ep`);
    if (item.season && item.seasonYear) {
      parts.push(`${item.season} ${item.seasonYear}`);
    } else if (item.seasonYear) {
      parts.push(`${item.seasonYear}`);
    } else if (item.status) {
      parts.push(item.status);
    }
    return parts.join(' \u00B7 ');
  }

  genreBadgeColor(index: number): BloomBadgeColor {
    return GENRE_BADGE_COLORS[index % GENRE_BADGE_COLORS.length];
  }

  // --- Add flow (two-step) ---

  startAdd(item: SearchResultItem): void {
    this.confirmingItem.set(item);
    this.updateItemState(item.anilistMediaId, 'confirm');
    this.resetAddFields();
  }

  cancelAdd(): void {
    const item = this.confirmingItem();
    if (item) {
      this.updateItemState(item.anilistMediaId, 'idle');
    }
    this.confirmingItem.set(null);
    this.resetAddFields();
  }

  confirmAdd(): void {
    const item = this.confirmingItem();
    if (!item) return;

    this.isAdding.set(true);
    this.addError.set('');
    this.updateItemState(item.anilistMediaId, 'adding');

    this.watchSpaceService
      .ensureMediaCached(item.anilistMediaId)
      .pipe(
        switchMap(() =>
          this.watchSpaceService.addAnimeToWatchSpace(this.watchSpaceId(), {
            aniListMediaId: item.anilistMediaId,
            mood: this.addMood.trim() || null,
            vibe: this.addVibe.trim() || null,
            pitch: this.addPitch.trim() || null,
          }),
        ),
      )
      .subscribe({
        next: () => {
          this.updateItemState(item.anilistMediaId, 'added');
          this.existingAniListIds.add(item.anilistMediaId);
          this.addedCount++;
          this.isAdding.set(false);
          this.confirmingItem.set(null);
          this.resetAddFields();
          this.animeAdded.emit();
        },
        error: () => {
          this.updateItemState(item.anilistMediaId, 'error');
          this.isAdding.set(false);
          this.addError.set('Failed to add. Please try again.');
        },
      });
  }

  private resetAddFields(): void {
    this.addMood = '';
    this.addVibe = '';
    this.addPitch = '';
    this.addError.set('');
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
    this.confirmingItem.set(null);
    this.resetAddFields();

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
