import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { Component } from '@angular/core';
import { WatchSpaceDashboard } from './watch-space-dashboard';
import { WatchSpaceDetail } from './watch-space-detail';
import { DashboardSummary } from './watch-space.model';
import { watchSpacesRoutes } from './watch-spaces.routes';

// ---------------------------------------------------------------------------
// Mock Data
// ---------------------------------------------------------------------------

const fullDashboard: DashboardSummary = {
  stats: {
    totalShows: 27,
    currentlyWatching: 3,
    finished: 11,
    episodesWatchedTogether: 184,
  },
  compatibility: {
    score: 87,
    averageGap: 1.3,
    ratedTogetherCount: 9,
    label: 'Very synced, with a little spice',
  },
  compatibilityMessage: null,
  currentlyWatching: [
    { watchSpaceAnimeId: 'a1', preferredTitle: 'Frieren', coverImageUrl: 'https://example.com/frieren.jpg', sharedEpisodesWatched: 5, episodeCountSnapshot: 28 },
    { watchSpaceAnimeId: 'a2', preferredTitle: 'Dandadan', coverImageUrl: null, sharedEpisodesWatched: 12, episodeCountSnapshot: 12 },
    { watchSpaceAnimeId: 'a3', preferredTitle: 'Blue Lock', coverImageUrl: null, sharedEpisodesWatched: 2, episodeCountSnapshot: 24 },
  ],
  backlogHighlights: [
    { watchSpaceAnimeId: 'b1', preferredTitle: 'Vinland Saga', coverImageUrl: null, format: 'TV' },
    { watchSpaceAnimeId: 'b2', preferredTitle: 'Bocchi the Rock!', coverImageUrl: 'https://example.com/bocchi.jpg', format: 'TV' },
  ],
  ratingGapHighlights: [
    {
      watchSpaceAnimeId: 'g1',
      preferredTitle: 'Jujutsu Kaisen',
      coverImageUrl: null,
      gap: 3.5,
      ratings: [
        { userId: 'u1', displayName: 'hazel', ratingScore: 9.5 },
        { userId: 'u2', displayName: 'sakura', ratingScore: 6.0 },
      ],
    },
    {
      watchSpaceAnimeId: 'g2',
      preferredTitle: 'Chainsaw Man',
      coverImageUrl: null,
      gap: 2.5,
      ratings: [
        { userId: 'u1', displayName: 'hazel', ratingScore: 8.0 },
        { userId: 'u2', displayName: 'sakura', ratingScore: 5.5 },
      ],
    },
  ],
};

const emptyDashboard: DashboardSummary = {
  stats: { totalShows: 0, currentlyWatching: 0, finished: 0, episodesWatchedTogether: 0 },
  compatibility: null,
  compatibilityMessage: 'Not enough data',
  currentlyWatching: [],
  backlogHighlights: [],
  ratingGapHighlights: [],
};

// Dummy component for route targets
@Component({ standalone: true, template: '' })
class DummyComponent {}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('WatchSpaceDashboard', () => {
  let fixture: ComponentFixture<WatchSpaceDashboard>;
  let component: WatchSpaceDashboard;
  let httpTesting: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      imports: [WatchSpaceDashboard],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          {
            path: 'watch-spaces',
            children: watchSpacesRoutes,
          },
        ]),
      ],
    });

    router = TestBed.inject(Router);
    httpTesting = TestBed.inject(HttpTestingController);

    // Navigate to the dashboard route to set up ActivatedRoute params
    await router.navigateByUrl('/watch-spaces/ws-1');

    fixture = TestBed.createComponent(WatchSpaceDashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function flushDashboard(data: DashboardSummary = fullDashboard): void {
    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/dashboard') && r.method === 'GET'
    );
    req.flush(data);
    fixture.detectChanges();
  }

  function flushError(): void {
    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/dashboard') && r.method === 'GET'
    );
    req.flush('Error', { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();
  }

  // ---- 10.1: Loading, Error, Retry ----

  it('should show loading skeletons initially', () => {
    const skeletons = fixture.nativeElement.querySelectorAll('.dashboard__skeleton');
    expect(skeletons.length).toBeGreaterThan(0);
    flushDashboard();
  });

  it('should render all sections after successful load', () => {
    flushDashboard();

    expect(fixture.nativeElement.querySelector('.dashboard__stats-grid')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.dashboard__compat')).toBeTruthy();
    expect(fixture.nativeElement.querySelectorAll('.dashboard__anime-thumb').length).toBeGreaterThan(0);
    expect(fixture.nativeElement.querySelectorAll('.dashboard__gap-card').length).toBeGreaterThan(0);
  });

  it('should show error state on API failure', () => {
    flushError();

    const error = fixture.nativeElement.querySelector('.dashboard__error');
    expect(error).toBeTruthy();
    expect(error.textContent).toContain('Failed to load dashboard');
  });

  it('should re-fetch on retry click', () => {
    flushError();

    const retryBtn = fixture.nativeElement.querySelector('.dashboard__retry');
    retryBtn.click();
    fixture.detectChanges();

    flushDashboard();
    const cards = fixture.nativeElement.querySelectorAll('.dashboard__stat-card');
    expect(cards.length).toBe(4);
  });

  // ---- 10.2: Stat Cards ----

  it('should display correct stat card values', () => {
    flushDashboard();

    const statNumbers = fixture.nativeElement.querySelectorAll('.dashboard__stat-number');
    expect(statNumbers[0].textContent.trim()).toBe('27');
    expect(statNumbers[1].textContent.trim()).toBe('3');
    expect(statNumbers[2].textContent.trim()).toBe('11');
    expect(statNumbers[3].textContent.trim()).toBe('184');
  });

  it('should display zeroes for empty space', () => {
    flushDashboard(emptyDashboard);

    const statNumbers = fixture.nativeElement.querySelectorAll('.dashboard__stat-number');
    expect(statNumbers[0].textContent.trim()).toBe('0');
    expect(statNumbers[1].textContent.trim()).toBe('0');
    expect(statNumbers[2].textContent.trim()).toBe('0');
    expect(statNumbers[3].textContent.trim()).toBe('0');
  });

  // ---- 10.3: Compatibility ----

  it('should render compatibility ring with score, label, and context', () => {
    flushDashboard();

    const score = fixture.nativeElement.querySelector('.dashboard__ring-score');
    expect(score?.textContent?.trim()).toBe('87');

    const label = fixture.nativeElement.querySelector('.dashboard__compat-label');
    expect(label?.textContent?.trim()).toBe('Very synced, with a little spice');

    const context = fixture.nativeElement.querySelector('.dashboard__compat-context');
    expect(context?.textContent).toContain('9 shared ratings');
  });

  it('should show placeholder when compatibility is null', () => {
    flushDashboard(emptyDashboard);

    const placeholder = fixture.nativeElement.querySelector('.dashboard__compat-placeholder');
    expect(placeholder).toBeTruthy();
    expect(placeholder.textContent).toContain('Not enough ratings yet');

    const ring = fixture.nativeElement.querySelector('.dashboard__ring-svg');
    expect(ring).toBeFalsy();
  });

  it('should use green color for score >= 80', () => {
    flushDashboard();
    expect(component.ringColor()).toContain('green');
  });

  it('should use yellow color for score 50-79', () => {
    const midDashboard = {
      ...fullDashboard,
      compatibility: { ...fullDashboard.compatibility!, score: 62 },
    };
    flushDashboard(midDashboard);
    expect(component.ringColor()).toContain('yellow');
  });

  it('should use pink color for score < 50', () => {
    const lowDashboard = {
      ...fullDashboard,
      compatibility: { ...fullDashboard.compatibility!, score: 35 },
    };
    flushDashboard(lowDashboard);
    expect(component.ringColor()).toContain('pink');
  });

  // ---- 10.4: Currently Watching ----

  it('should render currently-watching cards with progress bars', () => {
    flushDashboard();

    const section = fixture.nativeElement.querySelectorAll('.dashboard__section')[0];
    const thumbs = section.querySelectorAll('.dashboard__anime-thumb');
    expect(thumbs.length).toBe(3);

    // Frieren: has episodeCountSnapshot, should have progress bar
    const frierenThumb = thumbs[0];
    expect(frierenThumb.querySelector('.dashboard__progress-bar')).toBeTruthy();
    expect(frierenThumb.querySelector('.dashboard__anime-ep')?.textContent?.trim()).toBe('Ep 5 / 28');
  });

  it('should show empty state when no currently watching', () => {
    flushDashboard(emptyDashboard);

    const sections = fixture.nativeElement.querySelectorAll('.dashboard__section');
    const watchingSection = sections[0];
    const empty = watchingSection.querySelector('.dashboard__empty');
    expect(empty?.textContent).toContain('Nothing currently watching');
  });

  it('should navigate to anime detail on card click', () => {
    flushDashboard();
    const spy = vi.spyOn(router, 'navigate');

    const thumb = fixture.nativeElement.querySelector('.dashboard__anime-thumb');
    thumb.click();

    expect(spy).toHaveBeenCalledWith(['/watch-spaces', 'ws-1', 'anime', 'a1']);
  });

  // ---- 10.5: Backlog & Rating Gaps ----

  it('should render backlog highlight cards', () => {
    flushDashboard();

    const sections = fixture.nativeElement.querySelectorAll('.dashboard__section');
    const backlogSection = sections[1];
    const thumbs = backlogSection.querySelectorAll('.dashboard__anime-thumb');
    expect(thumbs.length).toBe(2);
    expect(thumbs[0].textContent).toContain('Vinland Saga');
  });

  it('should render rating gap entries with scores and delta', () => {
    flushDashboard();

    const gapCards = fixture.nativeElement.querySelectorAll('.dashboard__gap-card');
    expect(gapCards.length).toBe(2);

    const first = gapCards[0];
    expect(first.querySelector('.dashboard__gap-title')?.textContent?.trim()).toBe('Jujutsu Kaisen');
    expect(first.querySelector('.dashboard__gap-delta')?.textContent).toContain('3.5');

    const raters = first.querySelectorAll('.dashboard__gap-rater');
    expect(raters.length).toBe(2);
    expect(raters[0].textContent).toContain('hazel');
    expect(raters[0].textContent).toContain('9.5');
  });

  it('should show empty states for backlog and rating gaps', () => {
    flushDashboard(emptyDashboard);

    const empties = fixture.nativeElement.querySelectorAll('.dashboard__empty');
    // Currently watching, backlog, and rating gaps empty states
    expect(empties.length).toBe(3);
    expect(empties[1].textContent).toContain('No anime in backlog');
    expect(empties[2].textContent).toContain('No rating gaps yet');
  });

  // ---- 10.6: Header & Navigation ----

  it('should render header with navigation buttons', () => {
    flushDashboard();

    const header = fixture.nativeElement.querySelector('.dashboard__header');
    expect(header).toBeTruthy();

    const backLink = fixture.nativeElement.querySelector('.dashboard__back-link');
    expect(backLink?.textContent).toContain('Back to Watch Spaces');

    const actions = fixture.nativeElement.querySelector('.dashboard__header-actions');
    expect(actions?.textContent).toContain('Anime List');
  });

  it('should navigate to manage view on Anime List click', () => {
    flushDashboard();
    const spy = vi.spyOn(router, 'navigate');
    component.navigateToManage();
    expect(spy).toHaveBeenCalledWith(['/watch-spaces', 'ws-1', 'manage']);
  });
});
