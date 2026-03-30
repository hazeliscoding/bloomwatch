import { Component, inject, isDevMode, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ThemeService } from '../../theme/theme.service';
import { BloomThemeToggleComponent } from '../../../shared/ui/theme-toggle/bloom-theme-toggle';

@Component({
  selector: 'app-shell-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, BloomThemeToggleComponent],
  templateUrl: './shell-layout.html',
  styleUrl: './shell-layout.scss',
})
export class ShellLayout {
  private readonly themeService = inject(ThemeService);
  readonly mobileOpen = signal(false);
  readonly isDev = isDevMode();

  toggleMobile(): void {
    this.mobileOpen.update((v) => !v);
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
  }
}
