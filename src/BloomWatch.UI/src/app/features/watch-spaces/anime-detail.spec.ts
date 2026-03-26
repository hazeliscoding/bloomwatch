import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { AnimeDetail } from './anime-detail';
import { WatchSpaceAnimeDetail } from './watch-space.model';
import { AuthService } from '../../core/auth/auth.service';

const currentUserId = 'user-1';

const mockDetail: WatchSpaceAnimeDetail = {
  watchSpaceAnimeId: 'wsa-1',
  anilistMediaId: 1,
  preferredTitle: 'Cowboy Bebop',
  coverImageUrlSnapshot: 'https://example.com/bebop.jpg',
  episodeCountSnapshot: 26,
  format: 'TV',
  season: 'SPRING',
  seasonYear: 1998,
  sharedStatus: 'Watching',
  sharedEpisodesWatched: 12,
  mood: 'Chill',
  vibe: 'Jazzy',
  pitch: 'Space bounty hunters',
  addedByUserId: currentUserId,
  addedAtUtc: '2026-01-01T00:00:00Z',
  participants: [
    {
      userId: currentUserId,
      individualStatus: 'Watching',
      episodesWatched: 14,
      ratingScore: 9.0,
      ratingNotes: 'Masterpiece',
      lastUpdatedAtUtc: '2026-03-01T00:00:00Z',
    },
    {
      userId: 'user-2',
      individualStatus: 'Watching',
      episodesWatched: 10,
      ratingScore: null,
      ratingNotes: null,
      lastUpdatedAtUtc: '2026-03-02T00:00:00Z',
    },
  ],
};

const mockWatchSpaceDetail = {
  watchSpaceId: 'ws-1',
  name: 'Anime Club',
  createdAt: '2026-01-15T00:00:00Z',
  members: [
    { userId: currentUserId, displayName: 'Alice', role: 'Owner', joinedAt: '2026-01-15T00:00:00Z' },
    { userId: 'user-2', displayName: 'Bob', role: 'Member', joinedAt: '2026-02-01T00:00:00Z' },
  ],
};

function fakeJwt(sub: string): string {
  const header = btoa(JSON.stringify({ alg: 'HS256' }));
  const payload = btoa(JSON.stringify({ sub }));
  return `${header}.${payload}.signature`;
}

describe('AnimeDetail', () => {
  let fixture: ComponentFixture<AnimeDetail>;
  let component: AnimeDetail;
  let httpTesting: HttpTestingController;
  let authService: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AnimeDetail],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => {
                  if (key === 'id') return 'ws-1';
                  if (key === 'animeId') return 'wsa-1';
                  return null;
                },
              },
            },
          },
        },
      ],
    });

    authService = TestBed.inject(AuthService);
    authService.setToken(fakeJwt(currentUserId), '2099-01-01T00:00:00Z');

    fixture = TestBed.createComponent(AnimeDetail);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  function flushDetailAndMembers(detail: WatchSpaceAnimeDetail = mockDetail): void {
    const detailReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'GET'
    );
    detailReq.flush(detail);

    const membersReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1') && !r.url.includes('/anime') && r.method === 'GET'
    );
    membersReq.flush(mockWatchSpaceDetail);
    fixture.detectChanges();
  }

  // ---- Loading & Error States ----

  it('should show loading state initially', () => {
    const loading = fixture.nativeElement.querySelector('.anime-detail__loading');
    expect(loading).toBeTruthy();
    flushDetailAndMembers();
  });

  it('should show error state on API failure', () => {
    const detailReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'GET'
    );
    detailReq.flush('Error', { status: 500, statusText: 'Server Error' });

    const membersReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1') && !r.url.includes('/anime') && r.method === 'GET'
    );
    membersReq.flush(mockWatchSpaceDetail);
    fixture.detectChanges();

    const error = fixture.nativeElement.querySelector('.anime-detail__error');
    expect(error).toBeTruthy();
    expect(error.textContent).toContain('Failed to load anime details');
  });

  it('should retry on error retry button click', () => {
    const detailReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'GET'
    );
    detailReq.flush('Error', { status: 500, statusText: 'Server Error' });

    const membersReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1') && !r.url.includes('/anime') && r.method === 'GET'
    );
    membersReq.flush(mockWatchSpaceDetail);
    fixture.detectChanges();

    const retryBtn = fixture.nativeElement.querySelector('.anime-detail__error bloom-button');
    retryBtn.querySelector('button').click();
    fixture.detectChanges();

    flushDetailAndMembers();
    const hero = fixture.nativeElement.querySelector('.anime-detail__hero');
    expect(hero).toBeTruthy();
  });

  // ---- Hero Section ----

  it('should display anime title in hero section', () => {
    flushDetailAndMembers();
    const title = fixture.nativeElement.querySelector('.anime-detail__title');
    expect(title?.textContent).toContain('Cowboy Bebop');
  });

  it('should display format and season metadata', () => {
    flushDetailAndMembers();
    const meta = fixture.nativeElement.querySelectorAll('.anime-detail__meta-item');
    const metaTexts = Array.from(meta).map((m) => (m as HTMLElement).textContent?.trim());
    expect(metaTexts).toContain('TV');
    expect(metaTexts).toContain('SPRING 1998');
    expect(metaTexts).toContain('26 episodes');
  });

  it('should show cover image placeholder when coverImageUrlSnapshot is null', () => {
    flushDetailAndMembers({ ...mockDetail, coverImageUrlSnapshot: null });
    const placeholder = fixture.nativeElement.querySelector('.anime-detail__hero-cover-placeholder');
    expect(placeholder).toBeTruthy();
  });

  // ---- Shared Status ----

  it('should display shared status badge', () => {
    flushDetailAndMembers();
    const badge = fixture.nativeElement.querySelector('.anime-detail__shared-status bloom-badge');
    expect(badge?.textContent?.trim()).toContain('Watching');
  });

  it('should display episode progress', () => {
    flushDetailAndMembers();
    const progress = fixture.nativeElement.querySelector('.anime-detail__shared-progress');
    expect(progress?.textContent).toContain('Ep 12 / 26');
  });

  it('should display mood/vibe/pitch tags when present', () => {
    flushDetailAndMembers();
    const tags = fixture.nativeElement.querySelectorAll('.anime-detail__tag');
    expect(tags.length).toBe(3);
    const tagTexts = Array.from(tags).map((t) => (t as HTMLElement).textContent?.trim());
    expect(tagTexts.some((t) => t?.includes('Chill'))).toBe(true);
    expect(tagTexts.some((t) => t?.includes('Jazzy'))).toBe(true);
    expect(tagTexts.some((t) => t?.includes('Space bounty hunters'))).toBe(true);
  });

  it('should not display mood/vibe/pitch tags when null', () => {
    flushDetailAndMembers({ ...mockDetail, mood: null, vibe: null, pitch: null });
    const tags = fixture.nativeElement.querySelectorAll('.anime-detail__tag');
    expect(tags.length).toBe(0);
  });

  // ---- Participants ----

  it('should display participant cards with resolved display names', () => {
    flushDetailAndMembers();
    const cards = fixture.nativeElement.querySelectorAll('.anime-detail__participant-card');
    expect(cards.length).toBe(2);

    const names = fixture.nativeElement.querySelectorAll('.anime-detail__participant-name');
    expect(names[0]?.textContent?.trim()).toBe('Alice');
    expect(names[1]?.textContent?.trim()).toBe('Bob');
  });

  it('should display rating score when present', () => {
    flushDetailAndMembers();
    const scores = fixture.nativeElement.querySelectorAll('.anime-detail__rating-score');
    expect(scores[0]?.textContent).toContain('9 / 10');
  });

  it('should display "No rating" when ratingScore is null', () => {
    flushDetailAndMembers();
    const noRating = fixture.nativeElement.querySelectorAll('.anime-detail__rating-none');
    expect(noRating.length).toBe(1);
    expect(noRating[0]?.textContent).toContain('No rating');
  });

  // ---- Back Navigation ----

  it('should have a back navigation button', () => {
    flushDetailAndMembers();
    const backBtn = fixture.nativeElement.querySelector('.anime-detail__back-btn');
    expect(backBtn).toBeTruthy();
    expect(backBtn.textContent).toContain('Back to Watch Space');
  });

  // ---- Progress Form ----

  it('should toggle progress form on click', () => {
    flushDetailAndMembers();
    const toggle = fixture.nativeElement.querySelectorAll('.anime-detail__action-toggle')[0];
    toggle.click();
    fixture.detectChanges();

    const form = fixture.nativeElement.querySelector('#progress-status');
    expect(form).toBeTruthy();
  });

  it('should submit progress update', () => {
    flushDetailAndMembers();

    // Open form
    const toggle = fixture.nativeElement.querySelectorAll('.anime-detail__action-toggle')[0];
    toggle.click();
    fixture.detectChanges();

    // Submit
    component.progressStatus = 'Finished';
    component.progressEpisodes = 26;
    component.submitProgress();
    fixture.detectChanges();

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1/participant-progress') && r.method === 'PATCH'
    );
    expect(req.request.body).toEqual({ individualStatus: 'Finished', episodesWatched: 26 });
    req.flush({ userId: currentUserId, individualStatus: 'Finished', episodesWatched: 26, ratingScore: 9, ratingNotes: 'Masterpiece', lastUpdatedAtUtc: '2026-03-20T00:00:00Z' });

    // Re-fetch detail
    const refetchReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'GET'
    );
    refetchReq.flush(mockDetail);
    fixture.detectChanges();
  });

  // ---- Rating Form ----

  it('should toggle rating form on click', () => {
    flushDetailAndMembers();
    const toggle = fixture.nativeElement.querySelectorAll('.anime-detail__action-toggle')[1];
    toggle.click();
    fixture.detectChanges();

    const scoreInput = fixture.nativeElement.querySelector('#rating-score');
    expect(scoreInput).toBeTruthy();
  });

  it('should validate rating score range', () => {
    flushDetailAndMembers();
    component.ratingScore = 0;
    expect(component.isValidRating()).toBe(false);

    component.ratingScore = 11;
    expect(component.isValidRating()).toBe(false);

    component.ratingScore = 7.3;
    expect(component.isValidRating()).toBe(false);

    component.ratingScore = 7.5;
    expect(component.isValidRating()).toBe(true);
  });

  it('should validate rating notes length', () => {
    flushDetailAndMembers();
    component.ratingScore = 8;
    component.ratingNotes = 'x'.repeat(1001);
    expect(component.isValidRating()).toBe(false);

    component.ratingNotes = 'x'.repeat(1000);
    expect(component.isValidRating()).toBe(true);
  });

  // ---- Prefill Forms ----

  it('should prefill progress form with current user data', () => {
    flushDetailAndMembers();
    expect(component.progressStatus).toBe('Watching');
    expect(component.progressEpisodes).toBe(14);
  });

  it('should prefill rating form with current user data', () => {
    flushDetailAndMembers();
    expect(component.ratingScore).toBe(9.0);
    expect(component.ratingNotes).toBe('Masterpiece');
  });

  // ---- Shared Status Wiring ----

  it('should call updateSharedAnime when shared status changes', () => {
    flushDetailAndMembers();
    component.onSharedStatusChange('Finished');

    expect(component.anime()!.sharedStatus).toBe('Finished');

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'PATCH'
    );
    expect(req.request.body).toEqual({ sharedStatus: 'Finished' });
    req.flush({ ...mockDetail, sharedStatus: 'Finished' });
  });

  it('should rollback shared status on API failure', () => {
    flushDetailAndMembers();
    component.onSharedStatusChange('Dropped');

    expect(component.anime()!.sharedStatus).toBe('Dropped');

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'PATCH'
    );
    req.flush('Error', { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    expect(component.anime()!.sharedStatus).toBe('Watching');
    expect(component.sharedError()).toContain('Failed to update status');
  });

  // ---- Shared Episode Stepper Wiring ----

  it('should call updateSharedAnime when shared episode incremented', () => {
    flushDetailAndMembers();
    component.incrementSharedEpisode();

    expect(component.anime()!.sharedEpisodesWatched).toBe(13);

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'PATCH'
    );
    expect(req.request.body).toEqual({ sharedEpisodesWatched: 13 });
    req.flush({ ...mockDetail, sharedEpisodesWatched: 13 });
  });

  it('should call updateSharedAnime when shared episode decremented', () => {
    flushDetailAndMembers();
    component.decrementSharedEpisode();

    expect(component.anime()!.sharedEpisodesWatched).toBe(11);

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'PATCH'
    );
    expect(req.request.body).toEqual({ sharedEpisodesWatched: 11 });
    req.flush({ ...mockDetail, sharedEpisodesWatched: 11 });
  });

  it('should show inline error when shared episode update fails', () => {
    flushDetailAndMembers();
    component.incrementSharedEpisode();

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'PATCH'
    );
    req.flush('Error', { status: 500, statusText: 'Server Error' });

    // switchMap error triggers refreshDetail
    const refetchReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'GET'
    );
    refetchReq.flush(mockDetail);
    fixture.detectChanges();

    expect(component.sharedError()).toContain('Failed to update episodes');
  });
});
