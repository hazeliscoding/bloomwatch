import { Component, computed, inject } from '@angular/core';
import { ThemeService } from '../../../core/theme/theme.service';

@Component({
  selector: 'bloom-theme-toggle',
  standalone: true,
  template: `
    <button
      class="bloom-theme-toggle"
      type="button"
      (click)="themeService.toggle()"
      [attr.aria-label]="ariaLabel()"
    >
      <span class="bloom-theme-toggle__icon" aria-hidden="true">
        @if (isDark()) {
          &#9788;
        } @else {
          &#9790;
        }
      </span>
    </button>
  `,
  styleUrl: './bloom-theme-toggle.scss',
})
export class BloomThemeToggleComponent {
  protected readonly themeService = inject(ThemeService);
  protected readonly isDark = computed(() => this.themeService.theme() === 'dark');
  protected readonly ariaLabel = computed(() =>
    this.isDark() ? 'Switch to light mode' : 'Switch to dark mode'
  );
}
