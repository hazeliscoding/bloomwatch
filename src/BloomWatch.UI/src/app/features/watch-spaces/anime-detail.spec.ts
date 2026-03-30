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

  it('should display format and season metadata line', () => {
    flushDetailAndMembers();
    const metaLine = fixture.nativeElement.querySelector('.anime-detail__meta-line');
    expect(metaLine?.textContent).toContain('TV');
    expect(metaLine?.textContent).toContain('Spring 1998');
    expect(metaLine?.textContent).toContain('26 Episodes');
  });

  it('should show cover image placeholder when coverImageUrlSnapshot is null', () => {
    flushDetailAndMembers({ ...mockDetail, coverImageUrlSnapshot: null });
    const placeholder = fixture.nativeElement.querySelector('.anime-detail__hero-cover-placeholder');
    expect(placeholder).toBeTruthy();
  });

  // ---- Shared Status ----

  it('should display shared status select with current value', () => {
    flushDetailAndMembers();
    const select = fixture.nativeElement.querySelector('.anime-detail__status-select') as HTMLSelectElement;
    expect(select).toBeTruthy();
    expect(component.anime()!.sharedStatus).toBe('Watching');
  });

  it('should display shared episode stepper', () => {
    flushDetailAndMembers();
    const value = fixture.nativeElement.querySelector('.anime-detail__ep-stepper-value');
    expect(value?.textContent?.trim()).toBe('12');
    const total = fixture.nativeElement.querySelector('.anime-detail__ep-total');
    expect(total?.textContent).toContain('/ 26');
  });

  it('should display mood/vibe/pitch tags when present', () => {
    flushDetailAndMembers();
    const tags = fixture.nativeElement.querySelectorAll('.anime-detail__mood-tag');
    expect(tags.length).toBe(3);
    const tagTexts = Array.from(tags).map((t) => (t as HTMLElement).textContent?.trim());
    expect(tagTexts.some((t) => t?.includes('Chill'))).toBe(true);
    expect(tagTexts.some((t) => t?.includes('Jazzy'))).toBe(true);
    expect(tagTexts.some((t) => t?.includes('Space bounty hunters'))).toBe(true);
  });

  it('should not display mood/vibe/pitch tags when null', () => {
    flushDetailAndMembers({ ...mockDetail, mood: null, vibe: null, pitch: null });
    const tags = fixture.nativeElement.querySelectorAll('.anime-detail__mood-tag');
    expect(tags.length).toBe(0);
  });

  // ---- Participants ----

  it('should display participant cards with resolved display names', () => {
    flushDetailAndMembers();
    const cards = fixture.nativeElement.querySelectorAll('.anime-detail__participant-card');
    expect(cards.length).toBe(2);

    const names = fixture.nativeElement.querySelectorAll('.anime-detail__participant-name');
    expect(names[0]?.textContent?.trim()).toContain('Alice');
    expect(names[1]?.textContent?.trim()).toBe('Bob');
  });

  it('should display self card with Your progress badge', () => {
    flushDetailAndMembers();
    const selfCard = fixture.nativeElement.querySelector('.anime-detail__participant-card--self');
    expect(selfCard).toBeTruthy();

    const badge = selfCard.querySelector('.anime-detail__participant-label');
    expect(badge?.textContent?.trim()).toContain('Your progress');
  });

  it('should display rating score and stars for self', () => {
    flushDetailAndMembers();
    const selfCard = fixture.nativeElement.querySelector('.anime-detail__participant-card--self');
    const score = selfCard.querySelector('.anime-detail__rating-score');
    expect(score?.textContent).toContain('9');
  });

  it('should display other participant with read-only status badge', () => {
    flushDetailAndMembers();
    const otherCards = fixture.nativeElement.querySelectorAll('.anime-detail__participant-card:not(.anime-detail__participant-card--self)');
    expect(otherCards.length).toBe(1);

    const badge = otherCards[0].querySelector('bloom-badge');
    expect(badge?.textContent?.trim()).toContain('Watching');
  });

  it('should display italic notes for other participant', () => {
    const detailWithNotes: WatchSpaceAnimeDetail = {
      ...mockDetail,
      participants: [
        mockDetail.participants[0],
        { ...mockDetail.participants[1], ratingScore: 8, ratingNotes: 'Great show' },
      ],
    };
    flushDetailAndMembers(detailWithNotes);
    const notes = fixture.nativeElement.querySelector('.anime-detail__participant-notes');
    expect(notes?.textContent).toContain('Great show');
  });

  // ---- Enriched Metadata ----

  it('should display AniList score and popularity when present', () => {
    flushDetailAndMembers({ ...mockDetail, averageScore: 86, popularity: 12345 });
    const stats = fixture.nativeElement.querySelector('.anime-detail__anilist-stats');
    expect(stats?.textContent).toContain('86');
    expect(stats?.textContent).toContain('#12345');
  });

  it('should not display AniList stats when absent', () => {
    flushDetailAndMembers({ ...mockDetail });
    const stats = fixture.nativeElement.querySelector('.anime-detail__anilist-stats');
    expect(stats?.textContent?.trim()).toBe('');
  });

  it('should display airing status badge when present', () => {
    flushDetailAndMembers({ ...mockDetail, airingStatus: 'RELEASING' });
    const badge = fixture.nativeElement.querySelector('.anime-detail__airing-badge');
    expect(badge).toBeTruthy();
    expect(badge?.textContent?.trim()).toContain('Airing');
  });

  it('should not display airing status badge when absent', () => {
    flushDetailAndMembers({ ...mockDetail });
    const badge = fixture.nativeElement.querySelector('.anime-detail__airing-badge');
    expect(badge).toBeFalsy();
  });

  it('should display tags sorted by rank and limited to 15', () => {
    const tags = Array.from({ length: 20 }, (_, i) => ({
      name: `Tag ${i}`,
      rank: i * 5,
      isMediaSpoiler: false,
    }));
    flushDetailAndMembers({ ...mockDetail, tags });
    const tagBadges = fixture.nativeElement.querySelectorAll('.anime-detail__tag');
    expect(tagBadges.length).toBe(15);
    // First tag should be the highest ranked
    expect(tagBadges[0]?.textContent?.trim()).toBe('Tag 19');
  });

  it('should blur spoiler tags and reveal on click', () => {
    const tags = [
      { name: 'Action', rank: 90, isMediaSpoiler: false },
      { name: 'Plot Twist', rank: 80, isMediaSpoiler: true },
    ];
    flushDetailAndMembers({ ...mockDetail, tags });

    const spoilerTag = fixture.nativeElement.querySelector('.anime-detail__tag--spoiler');
    expect(spoilerTag).toBeTruthy();
    expect(spoilerTag?.textContent?.trim()).toBe('Plot Twist');

    spoilerTag.click();
    fixture.detectChanges();

    const revealed = fixture.nativeElement.querySelector('.anime-detail__tag--revealed');
    expect(revealed).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.anime-detail__tag--spoiler')).toBeFalsy();
  });

  it('should render description as HTML', () => {
    flushDetailAndMembers({ ...mockDetail, description: 'A <b>great</b> show about <i>space</i>.' });
    const desc = fixture.nativeElement.querySelector('.anime-detail__description');
    expect(desc).toBeTruthy();
    const bold = desc.querySelector('b');
    expect(bold?.textContent).toBe('great');
    const italic = desc.querySelector('i');
    expect(italic?.textContent).toBe('space');
  });

  it('should display AniList external link with siteUrl', () => {
    flushDetailAndMembers({ ...mockDetail, siteUrl: 'https://anilist.co/anime/1' });
    const link = fixture.nativeElement.querySelector('.anime-detail__anilist-link') as HTMLAnchorElement;
    expect(link).toBeTruthy();
    expect(link.textContent).toContain('View on AniList');
    expect(link.href).toBe('https://anilist.co/anime/1');
    expect(link.target).toBe('_blank');
  });

  it('should construct fallback AniList link from anilistMediaId', () => {
    flushDetailAndMembers({ ...mockDetail });
    const link = fixture.nativeElement.querySelector('.anime-detail__anilist-link') as HTMLAnchorElement;
    expect(link).toBeTruthy();
    expect(link.href).toContain('/anime/1');
  });

  // ---- Back Navigation ----

  it('should have a back navigation link', () => {
    flushDetailAndMembers();
    const backLink = fixture.nativeElement.querySelector('.anime-detail__back-link');
    expect(backLink).toBeTruthy();
    expect(backLink.textContent).toContain('Back to Anime List');
  });

  // ---- Rating Validation ----

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

  // ---- Progress Submit ----

  it('should submit progress update on episode increment', () => {
    flushDetailAndMembers();

    component.progressEpisodes = 15;
    component.submitProgress();

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1/participant-progress') && r.method === 'PATCH'
    );
    expect(req.request.body).toEqual({ individualStatus: 'Watching', episodesWatched: 15 });
    req.flush({ userId: currentUserId, individualStatus: 'Watching', episodesWatched: 15, ratingScore: 9, ratingNotes: 'Masterpiece', lastUpdatedAtUtc: '2026-03-20T00:00:00Z' });

    const refetchReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'GET'
    );
    refetchReq.flush(mockDetail);
    fixture.detectChanges();
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

    const refetchReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime/wsa-1') && r.method === 'GET'
    );
    refetchReq.flush(mockDetail);
    fixture.detectChanges();

    expect(component.sharedError()).toContain('Failed to update episodes');
  });
});
