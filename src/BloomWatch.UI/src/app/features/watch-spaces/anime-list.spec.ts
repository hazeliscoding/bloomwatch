import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { Component, signal, viewChild } from '@angular/core';
import { AnimeListComponent } from './anime-list';
import { WatchSpaceAnimeListItem } from './watch-space.model';

const mockAnimeList: WatchSpaceAnimeListItem[] = [
  {
    watchSpaceAnimeId: 'a1',
    anilistMediaId: 1,
    preferredTitle: 'Cowboy Bebop',
    coverImageUrlSnapshot: 'https://example.com/bebop.jpg',
    episodeCountSnapshot: 26,
    sharedStatus: 'Watching',
    sharedEpisodesWatched: 12,
    addedAtUtc: '2026-01-01T00:00:00Z',
    participants: [
      { userId: 'u1', displayName: 'Alice', individualStatus: 'Watching', episodesWatched: 14 },
      { userId: 'u2', displayName: 'Bob', individualStatus: 'Watching', episodesWatched: 10 },
    ],
  },
  {
    watchSpaceAnimeId: 'a2',
    anilistMediaId: 2,
    preferredTitle: 'Steins;Gate',
    coverImageUrlSnapshot: null,
    episodeCountSnapshot: 24,
    sharedStatus: 'Backlog',
    sharedEpisodesWatched: 0,
    addedAtUtc: '2026-02-01T00:00:00Z',
    participants: [],
  },
  {
    watchSpaceAnimeId: 'a3',
    anilistMediaId: 3,
    preferredTitle: 'One Piece',
    coverImageUrlSnapshot: 'https://example.com/op.jpg',
    episodeCountSnapshot: null,
    sharedStatus: 'Watching',
    sharedEpisodesWatched: 1100,
    addedAtUtc: '2026-03-01T00:00:00Z',
    participants: [
      { userId: 'u1', displayName: 'Alice', individualStatus: 'Watching', episodesWatched: 1100 },
    ],
  },
];

@Component({
  standalone: true,
  imports: [AnimeListComponent],
  template: `<app-anime-list #list [watchSpaceId]="spaceId()" />`,
})
class TestHostComponent {
  readonly spaceId = signal('ws-1');
  readonly list = viewChild.required<AnimeListComponent>('list');
}

describe('AnimeListComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;
  let httpTesting: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TestHostComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          { path: 'watch-spaces/:id/anime/:animeId', component: TestHostComponent },
        ]),
      ],
    });

    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function flushAnimeList(items: WatchSpaceAnimeListItem[] = mockAnimeList): void {
    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime') && r.method === 'GET'
    );
    req.flush({ items });
    fixture.detectChanges();
  }

  // ---- Loading & Data Fetch ----

  it('should show loading state initially', () => {
    const loading = fixture.nativeElement.querySelector('.anime-list__loading');
    expect(loading).toBeTruthy();
    flushAnimeList();
  });

  it('should render anime cards after loading', () => {
    flushAnimeList();
    const cards = fixture.nativeElement.querySelectorAll('.anime-list__card');
    expect(cards.length).toBe(3);
  });

  it('should show error state on API failure', () => {
    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime') && r.method === 'GET'
    );
    req.flush('Error', { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    const error = fixture.nativeElement.querySelector('.anime-list__error');
    expect(error).toBeTruthy();
  });

  it('should retry on error retry click', () => {
    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime') && r.method === 'GET'
    );
    req.flush('Error', { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    const retryBtn = fixture.nativeElement.querySelector('.anime-list__retry');
    retryBtn.click();
    fixture.detectChanges();

    flushAnimeList();
    const cards = fixture.nativeElement.querySelectorAll('.anime-list__card');
    expect(cards.length).toBe(3);
  });

  // ---- Empty State ----

  it('should show empty state when no anime', () => {
    flushAnimeList([]);
    const empty = fixture.nativeElement.querySelector('.anime-list__empty');
    expect(empty?.textContent).toContain('No anime yet');
  });

  // ---- Default Tab ----

  it('should default to All tab showing all anime', () => {
    flushAnimeList();
    const activeTab = fixture.nativeElement.querySelector('.anime-list__tab--active');
    expect(activeTab?.textContent).toContain('All');
    const cards = fixture.nativeElement.querySelectorAll('.anime-list__card');
    expect(cards.length).toBe(3);
  });

  // ---- Tab Filtering ----

  it('should filter to Watching tab', () => {
    flushAnimeList();
    const tabs = fixture.nativeElement.querySelectorAll('.anime-list__tab');
    // Watching is the 3rd tab (All, Backlog, Watching)
    const watchingTab = Array.from(tabs).find(
      (t) => (t as HTMLElement).textContent?.includes('Watching')
    ) as HTMLElement;
    watchingTab.click();
    fixture.detectChanges();

    const cards = fixture.nativeElement.querySelectorAll('.anime-list__card');
    expect(cards.length).toBe(2); // Cowboy Bebop & One Piece
  });

  it('should filter to Backlog tab', () => {
    flushAnimeList();
    const tabs = fixture.nativeElement.querySelectorAll('.anime-list__tab');
    const backlogTab = Array.from(tabs).find(
      (t) => (t as HTMLElement).textContent?.includes('Backlog')
    ) as HTMLElement;
    backlogTab.click();
    fixture.detectChanges();

    const cards = fixture.nativeElement.querySelectorAll('.anime-list__card');
    expect(cards.length).toBe(1);
    expect(cards[0].textContent).toContain('Steins;Gate');
  });

  it('should show tab-specific empty message for Finished', () => {
    flushAnimeList();
    const tabs = fixture.nativeElement.querySelectorAll('.anime-list__tab');
    const finishedTab = Array.from(tabs).find(
      (t) => (t as HTMLElement).textContent?.includes('Finished')
    ) as HTMLElement;
    finishedTab.click();
    fixture.detectChanges();

    const empty = fixture.nativeElement.querySelector('.anime-list__empty');
    expect(empty?.textContent).toContain('Nothing Finished yet');
  });

  // ---- Card Content ----

  it('should display episode progress with total', () => {
    flushAnimeList();
    const cards = fixture.nativeElement.querySelectorAll('.anime-list__card');
    expect(cards[0].textContent).toContain('Ep 12 / 26');
  });

  it('should display episode progress without total when null', () => {
    flushAnimeList();
    const cards = fixture.nativeElement.querySelectorAll('.anime-list__card');
    // One Piece has null episodeCountSnapshot
    const onePieceCard = Array.from(cards).find(
      (c) => (c as HTMLElement).textContent?.includes('One Piece')
    ) as HTMLElement;
    expect(onePieceCard.textContent).toContain('Ep 1100');
    expect(onePieceCard.textContent).not.toContain('Ep 1100 /');
  });

  it('should display participant progress', () => {
    flushAnimeList();
    const participants = fixture.nativeElement.querySelectorAll('.anime-list__card-participant');
    // Cowboy Bebop has 2, One Piece has 1 = 3 total
    expect(participants.length).toBe(3);
    expect(participants[0].textContent).toContain('Alice');
    expect(participants[0].textContent).toContain('Ep 14');
  });

  // ---- Navigation ----

  it('should navigate to detail page on card click', () => {
    flushAnimeList();
    const spy = vi.spyOn(router, 'navigate');
    const card = fixture.nativeElement.querySelector('.anime-list__card');
    card.click();
    expect(spy).toHaveBeenCalledWith(['/watch-spaces', 'ws-1', 'anime', 'a1']);
  });

  // ---- Refresh ----

  it('should re-fetch data when refresh() is called', () => {
    flushAnimeList();
    host.list().refresh();
    fixture.detectChanges();

    flushAnimeList([mockAnimeList[0]]);
    const cards = fixture.nativeElement.querySelectorAll('.anime-list__card');
    expect(cards.length).toBe(1);
  });

  // ---- Tab Counts ----

  it('should show counts per status tab', () => {
    flushAnimeList();
    const tabs = fixture.nativeElement.querySelectorAll('.anime-list__tab');
    const watchingTab = Array.from(tabs).find(
      (t) => (t as HTMLElement).textContent?.includes('Watching')
    ) as HTMLElement;
    const count = watchingTab.querySelector('.anime-list__tab-count');
    expect(count?.textContent?.trim()).toBe('2');
  });
});
