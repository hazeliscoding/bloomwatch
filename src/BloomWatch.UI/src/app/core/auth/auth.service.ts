import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../http/api.service';

const TOKEN_KEY = 'bloom_access_token';
const EXPIRES_KEY = 'bloom_token_expires_at';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly tokenSignal = signal<string | null>(null);

  readonly token = this.tokenSignal.asReadonly();

  readonly isAuthenticated = computed(() => {
    const tok = this.tokenSignal();
    if (!tok) return false;

    const expiresAt = localStorage.getItem(EXPIRES_KEY);
    if (!expiresAt) return false;

    return new Date(expiresAt).getTime() > Date.now();
  });

  readonly userId = computed(() => {
    const tok = this.tokenSignal();
    if (!tok) return null;
    try {
      const payload = JSON.parse(atob(tok.split('.')[1]));
      return (payload.sub ?? payload.nameid ?? null) as string | null;
    } catch {
      return null;
    }
  });

  constructor() {
    const stored = localStorage.getItem(TOKEN_KEY);
    if (stored) {
      this.tokenSignal.set(stored);
    }
  }

  setToken(token: string, expiresAt: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(EXPIRES_KEY, expiresAt);
    this.tokenSignal.set(token);
  }

  clearToken(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXPIRES_KEY);
    this.tokenSignal.set(null);
  }

  forgotPassword(email: string): Observable<void> {
    return this.api.post<void>('/auth/forgot-password', { email });
  }

  resetPassword(token: string, newPassword: string): Observable<void> {
    return this.api.post<void>('/auth/reset-password', { token, newPassword });
  }
}
