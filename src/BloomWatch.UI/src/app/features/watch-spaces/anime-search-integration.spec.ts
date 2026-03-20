import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { vi } from 'vitest';
import { WatchSpaceDetail } from './watch-space-detail';
import { WatchSpaceDetail as WatchSpaceDetailModel } from './watch-space.model';
import { AuthService } from '../../core/auth/auth.service';

describe('Anime Search Integration', () => {
  let fixture: ComponentFixture<WatchSpaceDetail>;
  let component: WatchSpaceDetail;
  let httpTesting: HttpTestingController;

  const ownerUserId = 'user-owner';

  const mockDetail: WatchSpaceDetailModel = {
    watchSpaceId: 'ws-1',
    name: 'Anime Club',
    createdAt: '2026-01-15T00:00:00Z',
    members: [
      { userId: ownerUserId, displayName: 'Owner', role: 'Owner', joinedAt: '2026-01-15T00:00:00Z' },
    ],
  };

  function fakeJwt(sub: string): string {
    const header = btoa(JSON.stringify({ alg: 'HS256' }));
    const payload = btoa(JSON.stringify({ sub }));
    return `${header}.${payload}.signature`;
  }

  beforeEach(() => {
    vi.useFakeTimers();

    TestBed.configureTestingModule({
      imports: [WatchSpaceDetail],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'ws-1' } } },
        },
      ],
    });

    const authService = TestBed.inject(AuthService);
    authService.setToken(fakeJwt(ownerUserId), '2099-01-01T00:00:00Z');

    fixture = TestBed.createComponent(WatchSpaceDetail);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);

    fixture.detectChanges();

    // Respond to initial detail load
    const detailReq = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/ws-1'));
    detailReq.flush(mockDetail);
    fixture.detectChanges();

    // Respond to invitations load
    const invReq = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/ws-1/invitations'));
    invReq.flush([]);
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
    httpTesting.verify();
    document.body.style.overflow = '';
  });

  it('should open search modal, search, add anime, then close', () => {
    // 1. Click "Add Anime" button
    const addAnimeBtn = fixture.nativeElement.querySelector(
      '.watch-space-detail__add-anime bloom-button button'
    );
    expect(addAnimeBtn).toBeTruthy();
    addAnimeBtn.click();
    fixture.detectChanges();

    // 2. Modal should be visible
    const modalTitle = fixture.nativeElement.querySelector('.anime-search__title');
    expect(modalTitle?.textContent).toContain('Search Anime');

    // 3. Type search query
    vi.advanceTimersByTime(100); // let modal open effects settle
    fixture.detectChanges();

    const searchInput = fixture.nativeElement.querySelector(
      'app-anime-search-modal bloom-input input'
    ) as HTMLInputElement;
    searchInput.value = 'Naruto';
    searchInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    vi.advanceTimersByTime(350);
    fixture.detectChanges();

    // 4. Respond to search
    const searchReq = httpTesting.expectOne((r) => r.url.includes('/api/anilist/search'));
    searchReq.flush([
      {
        anilistMediaId: 20,
        titleRomaji: 'Naruto',
        titleEnglish: 'Naruto',
        coverImageUrl: 'https://example.com/naruto.jpg',
        episodes: 220,
        status: 'FINISHED',
        format: 'TV',
        season: 'FALL',
        seasonYear: 2002,
        genres: ['Action', 'Adventure'],
      },
    ]);
    fixture.detectChanges();

    // 5. Click Add on the result
    const resultAddBtn = fixture.nativeElement.querySelector(
      '.anime-search__result-action bloom-button button'
    );
    resultAddBtn.click();
    fixture.detectChanges();

    const addReq = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1/anime'));
    expect(addReq.request.method).toBe('POST');
    addReq.flush({
      watchSpaceAnimeId: 'anime-20',
      preferredTitle: 'Naruto',
      episodeCountSnapshot: 220,
      coverImageUrlSnapshot: 'https://example.com/naruto.jpg',
      format: 'TV',
      season: 'FALL',
      seasonYear: 2002,
    });
    fixture.detectChanges();

    // 6. Should show "Added"
    const addedLabel = fixture.nativeElement.querySelector('.anime-search__result-added');
    expect(addedLabel).toBeTruthy();

    // 7. Close modal
    const closeBtn = fixture.nativeElement.querySelector('.bloom-modal__close');
    closeBtn.click();
    fixture.detectChanges();

    // Modal should be gone
    const modal = fixture.nativeElement.querySelector('.bloom-modal__backdrop');
    expect(modal).toBeNull();
  });
});
