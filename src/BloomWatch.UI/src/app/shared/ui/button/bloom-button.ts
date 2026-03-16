import { Component, input, output, computed } from '@angular/core';
import { NgClass } from '@angular/common';

export type BloomButtonVariant = 'primary' | 'secondary' | 'accent' | 'ghost' | 'danger';
export type BloomButtonSize = 'sm' | 'md' | 'lg';
export type BloomButtonType = 'button' | 'submit' | 'reset';

@Component({
  selector: 'bloom-button',
  standalone: true,
  imports: [NgClass],
  styleUrl: './bloom-button.scss',
  template: `
    <button
      [type]="type()"
      [disabled]="disabled() || loading()"
      [ngClass]="buttonClasses()"
      [attr.aria-disabled]="disabled() || loading()"
      [attr.aria-busy]="loading()"
      (click)="handleClick($event)"
    >
      <!-- Loading spinner -->
      @if (loading()) {
        <span class="bloom-btn__spinner" aria-hidden="true">
          <span class="bloom-btn__spinner-dot"></span>
          <span class="bloom-btn__spinner-dot"></span>
          <span class="bloom-btn__spinner-dot"></span>
        </span>
      }

      <!-- Icon slot (left) -->
      <span class="bloom-btn__icon bloom-btn__icon--left">
        <ng-content select="[bloomButtonIconLeft]" />
      </span>

      <!-- Label -->
      <span class="bloom-btn__label">
        <ng-content />
      </span>

      <!-- Icon slot (right) -->
      <span class="bloom-btn__icon bloom-btn__icon--right">
        <ng-content select="[bloomButtonIconRight]" />
      </span>
    </button>
  `,
})
export class BloomButtonComponent {
  readonly variant = input<BloomButtonVariant>('primary');
  readonly size = input<BloomButtonSize>('md');
  readonly type = input<BloomButtonType>('button');
  readonly disabled = input<boolean>(false);
  readonly loading = input<boolean>(false);
  readonly fullWidth = input<boolean>(false);

  readonly clicked = output<MouseEvent>();

  readonly buttonClasses = computed(() => {
    return {
      'bloom-btn': true,
      [`bloom-btn--${this.variant()}`]: true,
      [`bloom-btn--${this.size()}`]: true,
      'bloom-btn--loading': this.loading(),
      'bloom-btn--full-width': this.fullWidth(),
    };
  });

  handleClick(event: MouseEvent): void {
    if (!this.disabled() && !this.loading()) {
      this.clicked.emit(event);
    }
  }
}
