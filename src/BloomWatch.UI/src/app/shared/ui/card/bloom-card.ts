import { Component, input, computed } from '@angular/core';
import { NgClass } from '@angular/common';

export type BloomCardVariant = 'default' | 'highlighted';

@Component({
  selector: 'bloom-card',
  standalone: true,
  imports: [NgClass],
  styleUrl: './bloom-card.scss',
  template: `
    <article [ngClass]="cardClasses()" [attr.aria-label]="ariaLabel()">
      @if (highlighted()) {
        <div class="bloom-card__sparkle-border" aria-hidden="true"></div>
      }

      <div class="bloom-card__inner">
        <!-- Header slot -->
        <header class="bloom-card__header">
          <ng-content select="[bloomCardHeader]" />
        </header>

        <!-- Body / Default slot -->
        <div class="bloom-card__body">
          <ng-content />
        </div>

        <!-- Footer slot -->
        <footer class="bloom-card__footer">
          <ng-content select="[bloomCardFooter]" />
        </footer>
      </div>
    </article>
  `,
})
export class BloomCardComponent {
  readonly variant = input<BloomCardVariant>('default');
  readonly hoverable = input<boolean>(true);
  readonly ariaLabel = input<string | undefined>(undefined);

  readonly highlighted = computed(() => this.variant() === 'highlighted');

  readonly cardClasses = computed(() => {
    return {
      'bloom-card': true,
      [`bloom-card--${this.variant()}`]: true,
      'bloom-card--hoverable': this.hoverable(),
    };
  });
}
