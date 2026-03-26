import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { BloomBacklogPickerComponent } from './bloom-backlog-picker';
import { Component } from '@angular/core';
import { RandomPickResult } from '../../../features/watch-spaces/watch-space.model';

@Component({
  standalone: true,
  imports: [BloomBacklogPickerComponent],
  template: `<bloom-backlog-picker [spaceId]="spaceId" (picked)="onPicked($event)" />`,
})
class TestHost {
  spaceId = 'ws-1';
  pickedId: string | null = null;
  onPicked(id: string) {
    this.pickedId = id;
  }
}

const FULL_PICK: RandomPickResult = {
  pick: {
    watchSpaceAnimeId: 'a1',
    preferredTitle: 'Spy x Family',
    coverImageUrlSnapshot: 'https://example.com/spy.jpg',
    episodeCountSnapshot: 25,
    mood: 'Cozy',
    vibe: 'Weekend binge',
    pitch: 'A found family story',
  },
  message: null,
};

const NULL_FIELDS_PICK: RandomPickResult = {
  pick: {
    watchSpaceAnimeId: 'a2',
    preferredTitle: 'Mystery Show',
    coverImageUrlSnapshot: null,
    episodeCountSnapshot: 12,
    mood: null,
    vibe: null,
    pitch: null,
  },
  message: null,
};

const EMPTY_BACKLOG: RandomPickResult = {
  pick: null,
  message: 'Your backlog is empty — add some anime first!',
};

describe('BloomBacklogPickerComponent', () => {
  let fixture: ComponentFixture<TestHost>;
  let host: TestHost;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TestHost],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpTesting = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(TestHost);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function flushPick(result: RandomPickResult): void {
    const req = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1/analytics/random-pick'));
    req.flush(result);
    fixture.detectChanges();
  }

  it('should show loading skeleton initially', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.picker__skeleton')).toBeTruthy();
    // Flush to satisfy afterEach verify
    flushPick(FULL_PICK);
  });

  it('should render pick with full data', () => {
    flushPick(FULL_PICK);
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('.picker__title')?.textContent).toContain('Spy x Family');
    expect(el.querySelector('.picker__episodes')?.textContent).toContain('25 episodes');
    expect(el.querySelector('.picker__badge--mood')?.textContent).toContain('Cozy');
    expect(el.querySelector('.picker__badge--vibe')?.textContent).toContain('Weekend binge');
    expect(el.querySelector('.picker__pitch')?.textContent).toContain('A found family story');

    const img = el.querySelector('.picker__cover img') as HTMLImageElement;
    expect(img?.src).toContain('spy.jpg');
  });

  it('should handle null optional fields', () => {
    flushPick(NULL_FIELDS_PICK);
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('.picker__title')?.textContent).toContain('Mystery Show');
    expect(el.querySelector('.picker__badge--mood')).toBeNull();
    expect(el.querySelector('.picker__badge--vibe')).toBeNull();
    expect(el.querySelector('.picker__pitch')).toBeNull();
    expect(el.querySelector('.picker__cover-placeholder')).toBeTruthy();
  });

  it('should show empty backlog message', () => {
    flushPick(EMPTY_BACKLOG);
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('.picker__empty-message')?.textContent).toContain('Your backlog is empty');
    expect(el.querySelector('.picker__card')).toBeNull();
    expect(el.querySelector('.picker__btn--reroll')).toBeNull();
  });

  it('should reroll on button click', () => {
    flushPick(FULL_PICK);
    const el: HTMLElement = fixture.nativeElement;

    const rerollBtn = el.querySelector('.picker__btn--reroll') as HTMLButtonElement;
    rerollBtn.click();
    fixture.detectChanges();

    // Should show loading while fetching
    expect(el.querySelector('.picker__skeleton')).toBeTruthy();

    flushPick(NULL_FIELDS_PICK);
    expect(el.querySelector('.picker__title')?.textContent).toContain('Mystery Show');
  });

  it('should emit picked event on View Details click', () => {
    flushPick(FULL_PICK);
    const el: HTMLElement = fixture.nativeElement;

    const viewBtn = el.querySelector('.picker__btn--view') as HTMLButtonElement;
    viewBtn.click();
    fixture.detectChanges();

    expect(host.pickedId).toBe('a1');
  });

  it('should not render episode count when null', () => {
    const noPick: RandomPickResult = {
      pick: {
        watchSpaceAnimeId: 'a3',
        preferredTitle: 'Ongoing',
        coverImageUrlSnapshot: null,
        episodeCountSnapshot: null,
        mood: null,
        vibe: null,
        pitch: null,
      },
      message: null,
    };
    flushPick(noPick);
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('.picker__episodes')).toBeNull();
  });
});
