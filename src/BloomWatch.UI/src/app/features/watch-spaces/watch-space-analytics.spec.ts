import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { WatchSpaceAnalytics } from './watch-space-analytics';
import {
  CompatibilityScoreResult,
  RatingGapsResult,
  SharedStatsResult,
} from './watch-space.model';

// ---------------------------------------------------------------------------
// Mock Data
// ---------------------------------------------------------------------------

const fullCompat: CompatibilityScoreResult = {
  compatibility: {
    score: 87,
    averageGap: 1.3,
    ratedTogetherCount: 9,
    label: 'Very synced, with a little spice',
  },
  message: null,
};

const nullCompat: CompatibilityScoreResult = {
  compatibility: null,
  message: 'Need at least 2 members with shared ratings',
};

const fullGaps: RatingGapsResult = {
  items: [
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
  message: null,
};

const emptyGaps: RatingGapsResult = {
  items: [],
  message: 'No anime have been rated by multiple members yet',
};

const fullStats: SharedStatsResult = {
  totalEpisodesWatchedTogether: 184,
  totalFinished: 11,
  totalDropped: 1,
};

const emptyStats: SharedStatsResult = {
  totalEpisodesWatchedTogether: 0,
  totalFinished: 0,
  totalDropped: 0,
};

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('WatchSpaceAnalytics', () => {
  let fixture: ComponentFixture<WatchSpaceAnalytics>;
  let component: WatchSpaceAnalytics;
  let httpTesting: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [WatchSpaceAnalytics],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => (key === 'id' ? 'ws-1' : null),
              },
            },
          },
        },
      ],
    });

    router = TestBed.inject(Router);
    httpTesting = TestBed.inject(HttpTestingController);

    fixture = TestBed.createComponent(WatchSpaceAnalytics);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function flushAll(
    compat: CompatibilityScoreResult = fullCompat,
    gaps: RatingGapsResult = fullGaps,
    stats: SharedStatsResult = fullStats,
  ): void {
    httpTesting
      .expectOne((r) => r.url.includes('/analytics/compatibility'))
      .flush(compat);
    httpTesting
      .expectOne((r) => r.url.includes('/analytics/rating-gaps'))
      .flush(gaps);
    httpTesting
      .expectOne((r) => r.url.includes('/analytics/shared-stats'))
      .flush(stats);
    fixture.detectChanges();
  }

  function flushWithError(section: 'compat' | 'gaps' | 'stats'): void {
    const compatReq = httpTesting.expectOne((r) => r.url.includes('/analytics/compatibility'));
    const gapsReq = httpTesting.expectOne((r) => r.url.includes('/analytics/rating-gaps'));
    const statsReq = httpTesting.expectOne((r) => r.url.includes('/analytics/shared-stats'));

    if (section === 'compat') {
      compatReq.flush('Error', { status: 500, statusText: 'Server Error' });
      gapsReq.flush(fullGaps);
      statsReq.flush(fullStats);
    } else if (section === 'gaps') {
      compatReq.flush(fullCompat);
      gapsReq.flush('Error', { status: 500, statusText: 'Server Error' });
      statsReq.flush(fullStats);
    } else {
      compatReq.flush(fullCompat);
      gapsReq.flush(fullGaps);
      statsReq.flush('Error', { status: 500, statusText: 'Server Error' });
    }
    fixture.detectChanges();
  }

  // ---- Loading State ----

  it('should show loading skeletons initially', () => {
    const skeletons = fixture.nativeElement.querySelectorAll('.analytics__skeleton');
    expect(skeletons.length).toBeGreaterThan(0);
    flushAll();
  });

  // ---- Header ----

  it('should render header with back link and title', () => {
    flushAll();

    const backLink = fixture.nativeElement.querySelector('.analytics__back-link');
    expect(backLink?.textContent).toContain('Back to Dashboard');

    const title = fixture.nativeElement.querySelector('.analytics__title');
    expect(title?.textContent?.trim()).toBe('Analytics');
  });

  // ---- Compatibility Section ----

  it('should render compatibility ring and breakdown when data is available', () => {
    flushAll();

    const ring = fixture.nativeElement.querySelector('bloom-compat-ring');
    expect(ring).toBeTruthy();

    const breakdown = fixture.nativeElement.querySelector('.analytics__breakdown');
    expect(breakdown).toBeTruthy();
    expect(breakdown.textContent).toContain('1.3 points');
    expect(breakdown.textContent).toContain('9');
  });

  it('should render compat ring placeholder and hide breakdown when compatibility is null', () => {
    flushAll(nullCompat);

    const ring = fixture.nativeElement.querySelector('bloom-compat-ring');
    expect(ring).toBeTruthy();

    const breakdown = fixture.nativeElement.querySelector('.analytics__breakdown');
    expect(breakdown).toBeFalsy();
  });

  // ---- Shared Stats Section ----

  it('should render shared stats grid with values', () => {
    flushAll();

    const statValues = fixture.nativeElement.querySelectorAll('.analytics__stat-value');
    expect(statValues.length).toBe(3);
    expect(statValues[0].textContent.trim()).toBe('184');
    expect(statValues[1].textContent.trim()).toBe('11');
    expect(statValues[2].textContent.trim()).toBe('1');
  });

  // ---- Rating Gaps Section ----

  it('should render rating gap rows with scores and delta', () => {
    flushAll();

    const gapRows = fixture.nativeElement.querySelectorAll('.analytics__gap-row');
    expect(gapRows.length).toBe(2);

    const firstRow = gapRows[0];
    expect(firstRow.querySelector('.analytics__gap-title')?.textContent?.trim()).toBe('Jujutsu Kaisen');
    expect(firstRow.querySelector('.analytics__gap-delta')?.textContent).toContain('3.5');

    const scores = firstRow.querySelectorAll('.analytics__gap-rater-score');
    expect(scores.length).toBe(2);
  });

  it('should show empty state when no rating gaps', () => {
    flushAll(fullCompat, emptyGaps);

    const empty = fixture.nativeElement.querySelector('.analytics__empty');
    expect(empty?.textContent).toContain('No anime have been rated');
  });

  it('should render legend with rater names', () => {
    flushAll();

    const legend = fixture.nativeElement.querySelector('.analytics__legend');
    expect(legend).toBeTruthy();
    expect(legend.textContent).toContain('hazel');
    expect(legend.textContent).toContain('sakura');
  });

  // ---- Chart Section ----

  it('should render chart when rating gaps data is available', () => {
    flushAll();

    const canvas = fixture.nativeElement.querySelector('canvas');
    expect(canvas).toBeTruthy();
  });

  it('should not render chart when rating gaps are empty', () => {
    flushAll(fullCompat, emptyGaps);

    const canvas = fixture.nativeElement.querySelector('canvas');
    expect(canvas).toBeFalsy();
  });

  // ---- Error States ----

  it('should show compatibility error with retry button when compat fails', () => {
    flushWithError('compat');

    const errors = fixture.nativeElement.querySelectorAll('.analytics__section-error');
    expect(errors.length).toBeGreaterThan(0);
    expect(errors[0].textContent).toContain('Failed to load compatibility');

    // Other sections should still render
    const statValues = fixture.nativeElement.querySelectorAll('.analytics__stat-value');
    expect(statValues.length).toBe(3);
  });

  it('should show stats error when stats endpoint fails', () => {
    flushWithError('stats');

    const errors = fixture.nativeElement.querySelectorAll('.analytics__section-error');
    const errorTexts = Array.from(errors).map((e: any) => e.textContent);
    expect(errorTexts.some((t: string) => t.includes('Failed to load shared stats'))).toBe(true);
  });

  it('should show gaps error when gaps endpoint fails', () => {
    flushWithError('gaps');

    const errors = fixture.nativeElement.querySelectorAll('.analytics__section-error');
    const errorTexts = Array.from(errors).map((e: any) => e.textContent);
    expect(errorTexts.some((t: string) => t.includes('Failed to load rating gaps'))).toBe(true);
  });
});
