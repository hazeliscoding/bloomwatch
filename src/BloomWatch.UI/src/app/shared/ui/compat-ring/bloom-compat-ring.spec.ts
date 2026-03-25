import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { BloomCompatRingComponent } from './bloom-compat-ring';
import { DashboardCompatibility } from '../../../features/watch-spaces/watch-space.model';

@Component({
  standalone: true,
  imports: [BloomCompatRingComponent],
  template: `<bloom-compat-ring [compatibility]="compat()" />`,
})
class TestHostComponent {
  readonly compat = signal<DashboardCompatibility | null>(null);
}

describe('BloomCompatRingComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TestHostComponent],
    });
    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
  });

  function setCompat(compat: DashboardCompatibility | null): void {
    host.compat.set(compat);
    fixture.detectChanges();
  }

  const highCompat: DashboardCompatibility = {
    score: 87,
    averageGap: 1.3,
    ratedTogetherCount: 9,
    label: 'Very synced, with a little spice',
  };

  // ---- Ring Rendering ----

  it('should render SVG ring with score when compatibility is provided', () => {
    setCompat(highCompat);

    const svg = fixture.nativeElement.querySelector('.compat-ring__svg');
    expect(svg).toBeTruthy();

    const score = fixture.nativeElement.querySelector('.compat-ring__score');
    expect(score?.textContent?.trim()).toBe('87');
  });

  it('should display label text', () => {
    setCompat(highCompat);

    const label = fixture.nativeElement.querySelector('.compat-ring__label');
    expect(label?.textContent?.trim()).toBe('Very synced, with a little spice');
  });

  it('should display rated-together context', () => {
    setCompat(highCompat);

    const context = fixture.nativeElement.querySelector('.compat-ring__context');
    expect(context?.textContent).toContain('9 shared ratings');
  });

  // ---- Color Ranges ----

  it('should use green color for score >= 80', () => {
    setCompat(highCompat);

    const fill = fixture.nativeElement.querySelector('.compat-ring__fill');
    expect(fill?.getAttribute('stroke')).toContain('green');
  });

  it('should use yellow color for score 50-79', () => {
    setCompat({ ...highCompat, score: 62 });

    const fill = fixture.nativeElement.querySelector('.compat-ring__fill');
    expect(fill?.getAttribute('stroke')).toContain('yellow');
  });

  it('should use pink color for score below 50', () => {
    setCompat({ ...highCompat, score: 35 });

    const fill = fixture.nativeElement.querySelector('.compat-ring__fill');
    expect(fill?.getAttribute('stroke')).toContain('pink');
  });

  // ---- Null / Placeholder ----

  it('should show placeholder when compatibility is null', () => {
    setCompat(null);

    const placeholder = fixture.nativeElement.querySelector('.compat-ring__placeholder');
    expect(placeholder).toBeTruthy();
    expect(placeholder.textContent).toContain('Rate more anime together');

    const svg = fixture.nativeElement.querySelector('.compat-ring__svg');
    expect(svg).toBeFalsy();
  });

  // ---- Edge Cases ----

  it('should render score of 0', () => {
    setCompat({ ...highCompat, score: 0 });

    const score = fixture.nativeElement.querySelector('.compat-ring__score');
    expect(score?.textContent?.trim()).toBe('0');

    const fill = fixture.nativeElement.querySelector('.compat-ring__fill');
    expect(fill?.getAttribute('stroke')).toContain('pink');
  });

  it('should render score of 100', () => {
    setCompat({ ...highCompat, score: 100 });

    const score = fixture.nativeElement.querySelector('.compat-ring__score');
    expect(score?.textContent?.trim()).toBe('100');

    const fill = fixture.nativeElement.querySelector('.compat-ring__fill');
    expect(fill?.getAttribute('stroke')).toContain('green');
  });
});
