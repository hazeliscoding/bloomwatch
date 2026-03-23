import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component, signal } from '@angular/core';
import { vi } from 'vitest';
import { AnimeSearchModalComponent } from './anime-search-modal';
import { AnimeSearchResult } from './watch-space.model';

const mockResults: AnimeSearchResult[] = [
  {
    anilistMediaId: 1,
    titleRomaji: 'Cowboy Bebop',
    titleEnglish: 'Cowboy Bebop',
    coverImageUrl: 'https://example.com/cover.jpg',
    episodes: 26,
    status: 'FINISHED',
    format: 'TV',
    season: 'SPRING',
    seasonYear: 1998,
    genres: ['Action', 'Adventure', 'Drama', 'Sci-Fi'],
  },
  {
    anilistMediaId: 2,
    titleRomaji: 'Kaubōi Bibappu: Tengoku no Tobira',
    titleEnglish: null,
    coverImageUrl: null,
    episodes: null,
    status: 'FINISHED',
    format: 'MOVIE',
    season: null,
    seasonYear: 2001,
    genres: ['Action', 'Drama'],
  },
];

@Component({
  standalone: true,
  imports: [AnimeSearchModalComponent],
  template: `
    <app-anime-search-modal
      [open]="isOpen()"
      [watchSpaceId]="'ws-1'"
      (closed)="onClosed()"
      (animeAdded)="onAdded()"
    />
  `,
})
class TestHostComponent {
  readonly isOpen = signal(false);
  closedCount = 0;
  addedCount = 0;

  onClosed(): void {
    this.closedCount++;
    this.isOpen.set(false);
  }

  onAdded(): void {
    this.addedCount++;
  }
}

describe('AnimeSearchModalComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    vi.useFakeTimers();

    TestBed.configureTestingModule({
      imports: [TestHostComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
    httpTesting.verify();
    document.body.style.overflow = '';
  });

  function openModal(): void {
    host.isOpen.set(true);
    fixture.detectChanges();
    vi.advanceTimersByTime(100);
    fixture.detectChanges();

    // Modal fetches existing anime on open
    const listReq = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1/anime') && r.method === 'GET');
    listReq.flush({ items: [] });
    fixture.detectChanges();
  }

  function typeAndSearch(query: string): void {
    openModal();

    const input = fixture.nativeElement.querySelector('bloom-input input') as HTMLInputElement;
    input.value = query;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    vi.advanceTimersByTime(350);
    fixture.detectChanges();
  }

  it('should not render modal content when closed', () => {
    const modal = fixture.nativeElement.querySelector('.bloom-modal__backdrop');
    expect(modal).toBeNull();
  });

  it('should render modal when open', () => {
    openModal();

    const title = fixture.nativeElement.querySelector('.anime-search__title');
    expect(title?.textContent).toContain('Search Anime');
  });

  it('should debounce search input and call API', () => {
    typeAndSearch('Cowboy');

    const req = httpTesting.expectOne((r) => r.url.includes('/api/anilist/search'));
    expect(req.request.url).toContain('query=Cowboy');
    req.flush(mockResults);
    fixture.detectChanges();

    const resultItems = fixture.nativeElement.querySelectorAll('.anime-search__result');
    expect(resultItems.length).toBe(2);
  });

  it('should display preferred English title with romaji fallback', () => {
    typeAndSearch('Cowboy');

    const req = httpTesting.expectOne((r) => r.url.includes('/api/anilist/search'));
    req.flush(mockResults);
    fixture.detectChanges();

    const titles = fixture.nativeElement.querySelectorAll('.anime-search__result-title');
    expect(titles[0]?.textContent).toContain('Cowboy Bebop');
    expect(titles[1]?.textContent).toContain('Kaubōi Bibappu');
  });

  it('should show empty state when no results', () => {
    typeAndSearch('xyznonexistent');

    const req = httpTesting.expectOne((r) => r.url.includes('/api/anilist/search'));
    req.flush([]);
    fixture.detectChanges();

    const empty = fixture.nativeElement.querySelector('.anime-search__empty');
    expect(empty).toBeTruthy();
  });

  it('should show error state on search failure', () => {
    typeAndSearch('Cowboy');

    const req = httpTesting.expectOne((r) => r.url.includes('/api/anilist/search'));
    req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
    fixture.detectChanges();

    const error = fixture.nativeElement.querySelector('.anime-search__error');
    expect(error).toBeTruthy();
  });

  it('should add anime and show success state', () => {
    typeAndSearch('Cowboy');

    const searchReq = httpTesting.expectOne((r) => r.url.includes('/api/anilist/search'));
    searchReq.flush(mockResults);
    fixture.detectChanges();

    // Step 1: click "+ Add" to open confirm panel
    const addBtn = fixture.nativeElement.querySelector('.anime-search__result-action bloom-button button');
    addBtn.click();
    fixture.detectChanges();

    // Confirm panel should appear
    const confirmPanel = fixture.nativeElement.querySelector('.anime-search__add-details');
    expect(confirmPanel).toBeTruthy();

    // Step 2: click "Add to Space" to confirm
    const confirmBtn = fixture.nativeElement.querySelector('.anime-search__add-actions bloom-button[variant="primary"] button');
    confirmBtn.click();
    fixture.detectChanges();

    // First: ensure media is cached via detail endpoint
    const cacheReq = httpTesting.expectOne((r) => r.url.includes('/api/anilist/media/1'));
    expect(cacheReq.request.method).toBe('GET');
    cacheReq.flush({});
    fixture.detectChanges();

    // Then: add anime to watchspace
    const addReq = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1/anime'));
    expect(addReq.request.method).toBe('POST');
    addReq.flush({
      watchSpaceAnimeId: 'anime-1',
      preferredTitle: 'Cowboy Bebop',
      episodeCountSnapshot: 26,
      coverImageUrlSnapshot: 'https://example.com/cover.jpg',
      format: 'TV',
      season: 'SPRING',
      seasonYear: 1998,
    });
    fixture.detectChanges();

    const addedBadge = fixture.nativeElement.querySelector('.anime-search__result--added bloom-badge');
    expect(addedBadge).toBeTruthy();
    expect(host.addedCount).toBe(1);
  });

  it('should limit genres to 3', () => {
    typeAndSearch('Cowboy');

    const req = httpTesting.expectOne((r) => r.url.includes('/api/anilist/search'));
    req.flush(mockResults);
    fixture.detectChanges();

    const firstResult = fixture.nativeElement.querySelectorAll('.anime-search__result')[0];
    const genreBadges = firstResult.querySelectorAll('.anime-search__result-genres bloom-badge');
    expect(genreBadges.length).toBe(3);
  });

  it('should mark already-added anime in search results', () => {
    host.isOpen.set(true);
    fixture.detectChanges();
    vi.advanceTimersByTime(100);
    fixture.detectChanges();

    // Return existing anime that includes anilistMediaId 1
    const listReq = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1/anime') && r.method === 'GET');
    listReq.flush({
      items: [
        { watchSpaceAnimeId: 'a1', anilistMediaId: 1, preferredTitle: 'Cowboy Bebop', coverImageUrlSnapshot: null, episodeCountSnapshot: 26, sharedStatus: 'Backlog', sharedEpisodesWatched: 0, addedAtUtc: '2026-01-01T00:00:00Z' },
      ],
    });
    fixture.detectChanges();

    // Now search
    const input = fixture.nativeElement.querySelector('bloom-input input') as HTMLInputElement;
    input.value = 'Cowboy';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    vi.advanceTimersByTime(350);
    fixture.detectChanges();

    const searchReq = httpTesting.expectOne((r) => r.url.includes('/api/anilist/search'));
    searchReq.flush(mockResults);
    fixture.detectChanges();

    // First result (anilistMediaId=1) should show as added
    const addedBadge = fixture.nativeElement.querySelector('.anime-search__result--added bloom-badge');
    expect(addedBadge).toBeTruthy();

    // Second result (anilistMediaId=2) should still have Add button
    const results = fixture.nativeElement.querySelectorAll('.anime-search__result');
    const secondAction = results[1].querySelector('.anime-search__result-action bloom-button');
    expect(secondAction).toBeTruthy();
  });
});
