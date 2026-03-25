import { Component, computed, input } from '@angular/core';
import { DashboardCompatibility } from '../../../features/watch-spaces/watch-space.model';

const RING_RADIUS = 56;
const RING_CIRCUMFERENCE = 2 * Math.PI * RING_RADIUS;

@Component({
  selector: 'bloom-compat-ring',
  standalone: true,
  styleUrl: './bloom-compat-ring.scss',
  template: `
    @if (compatibility(); as compat) {
      <div class="compat-ring">
        <svg class="compat-ring__svg" viewBox="0 0 140 140">
          <circle class="compat-ring__track" cx="70" cy="70" r="56" />
          <circle
            class="compat-ring__fill"
            cx="70" cy="70" r="56"
            [attr.stroke]="ringColor()"
            [attr.stroke-dasharray]="circumference"
            [attr.stroke-dashoffset]="ringStrokeDashoffset()"
          />
          <text class="compat-ring__score" x="70" y="70">{{ compat.score }}</text>
        </svg>
        <p class="compat-ring__label">{{ compat.label }}</p>
        <p class="compat-ring__context">Based on {{ compat.ratedTogetherCount }} shared ratings</p>
      </div>
    } @else {
      <div class="compat-ring__placeholder">
        <p>Rate more anime together to unlock your compatibility score</p>
      </div>
    }
  `,
})
export class BloomCompatRingComponent {
  readonly compatibility = input<DashboardCompatibility | null>(null);

  readonly circumference = RING_CIRCUMFERENCE;

  readonly ringStrokeDashoffset = computed(() => {
    const compat = this.compatibility();
    if (!compat) return RING_CIRCUMFERENCE;
    return RING_CIRCUMFERENCE * (1 - compat.score / 100);
  });

  readonly ringColor = computed(() => {
    const score = this.compatibility()?.score ?? 0;
    if (score >= 80) return 'var(--bloom-green-400)';
    if (score >= 50) return 'var(--bloom-yellow-400)';
    return 'var(--bloom-pink-400)';
  });
}
