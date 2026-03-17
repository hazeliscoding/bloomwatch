import { Injectable, signal, OnDestroy } from '@angular/core';

export type Theme = 'light' | 'dark';

const STORAGE_KEY = 'bloom-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService implements OnDestroy {
  readonly theme = signal<Theme>(this.resolveInitialTheme());

  private mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
  private mediaListener = (e: MediaQueryListEvent) => this.onSystemPreferenceChange(e);

  constructor() {
    this.applyTheme(this.theme());
    this.mediaQuery.addEventListener('change', this.mediaListener);
  }

  ngOnDestroy(): void {
    this.mediaQuery.removeEventListener('change', this.mediaListener);
  }

  toggle(): void {
    this.setTheme(this.theme() === 'light' ? 'dark' : 'light');
  }

  setTheme(theme: Theme): void {
    this.theme.set(theme);
    this.applyTheme(theme);
    this.persist(theme);
  }

  private resolveInitialTheme(): Theme {
    const stored = this.readStored();
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  private applyTheme(theme: Theme): void {
    document.documentElement.setAttribute('data-theme', theme);
  }

  private persist(theme: Theme): void {
    try {
      localStorage.setItem(STORAGE_KEY, theme);
    } catch {
      // localStorage unavailable (e.g. private browsing) — silently ignore
    }
  }

  private readStored(): string | null {
    try {
      return localStorage.getItem(STORAGE_KEY);
    } catch {
      return null;
    }
  }

  private onSystemPreferenceChange(e: MediaQueryListEvent): void {
    // Only follow system preference if user hasn't explicitly chosen
    if (this.readStored() === null) {
      this.setTheme(e.matches ? 'dark' : 'light');
    }
  }
}
