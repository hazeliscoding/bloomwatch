import { Component, input, computed } from '@angular/core';
import { NgClass } from '@angular/common';

export type BloomBadgeColor = 'pink' | 'blue' | 'green' | 'lilac' | 'yellow' | 'neutral';
export type BloomBadgeSize = 'sm' | 'md';

@Component({
  selector: 'bloom-badge',
  standalone: true,
  imports: [NgClass],
  styleUrl: './bloom-badge.scss',
  template: `
    <span [ngClass]="badgeClasses()" [attr.aria-label]="ariaLabel()">
      @if (dot()) {
        <span class="bloom-badge__dot" aria-hidden="true"></span>
      }
      <span class="bloom-badge__label">
        <ng-content />
      </span>
    </span>
  `,
})
export class BloomBadgeComponent {
  readonly color = input<BloomBadgeColor>('pink');
  readonly size = input<BloomBadgeSize>('md');
  readonly dot = input<boolean>(false);
  readonly ariaLabel = input<string | undefined>(undefined);

  readonly badgeClasses = computed(() => {
    return {
      'bloom-badge': true,
      [`bloom-badge--${this.color()}`]: true,
      [`bloom-badge--${this.size()}`]: true,
      'bloom-badge--dot': this.dot(),
    };
  });
}
