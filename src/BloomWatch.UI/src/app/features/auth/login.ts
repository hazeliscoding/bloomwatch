import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomInputComponent } from '../../shared/ui/input/bloom-input';
import { ApiService } from '../../core/http/api.service';
import { AuthService } from '../../core/auth/auth.service';

interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

@Component({
  selector: 'app-login',
  imports: [RouterLink, BloomButtonComponent, BloomInputComponent],
  styleUrl: './login.scss',
  template: `
    <main class="login">
      <div class="login__card">
        <div class="login__header">
          <h1 class="login__title bloom-font-display">Welcome Back</h1>
          <p class="login__subtitle">Sign in to continue tracking anime with your friends.</p>
        </div>

        @if (formError()) {
          <div class="login__error-banner" role="alert">
            {{ formError() }}
          </div>
        }

        <form (submit)="onSubmit($event)" novalidate>
          <div class="login__fields">
            <bloom-input
              label="Email"
              type="email"
              [required]="true"
              autocomplete="email"
              [error]="emailError()"
              (valueChange)="onEmailChange($event)"
              (blurred)="markTouched('email')"
            />

            <bloom-input
              label="Password"
              type="password"
              [required]="true"
              autocomplete="current-password"
              [error]="passwordError()"
              (valueChange)="onPasswordChange($event)"
              (blurred)="markTouched('password')"
            />
          </div>

          <div class="login__submit">
            <bloom-button
              type="submit"
              variant="primary"
              [fullWidth]="true"
              [disabled]="!isValid() || isSubmitting()"
              [loading]="isSubmitting()"
            >
              Sign In
            </bloom-button>
          </div>
        </form>

        <p class="login__footer">
          Don't have an account? <a routerLink="/register">Create one</a>
        </p>
      </div>
    </main>
  `,
})
export class Login {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  // Form field signals
  readonly email = signal('');
  readonly password = signal('');

  // Touched tracking
  readonly touchedFields = signal(new Set<string>());
  readonly submitAttempted = signal(false);

  // Submission state
  readonly isSubmitting = signal(false);
  readonly formError = signal('');

  // Validation: email
  readonly emailError = computed(() => {
    if (!this.isFieldVisible('email')) return '';
    const value = this.email().trim();
    if (!value) return 'Email is required';
    if (!EMAIL_PATTERN.test(value)) return 'Please enter a valid email address';
    return '';
  });

  // Validation: password
  readonly passwordError = computed(() => {
    if (!this.isFieldVisible('password')) return '';
    const value = this.password();
    if (!value) return 'Password is required';
    return '';
  });

  // Overall validity
  readonly isValid = computed(() => {
    const em = this.email().trim();
    const pw = this.password();
    return em.length > 0 && EMAIL_PATTERN.test(em) && pw.length > 0;
  });

  markTouched(field: string): void {
    this.touchedFields.update(set => {
      const next = new Set(set);
      next.add(field);
      return next;
    });
  }

  onEmailChange(value: string): void {
    this.email.set(value);
    this.formError.set('');
  }

  onPasswordChange(value: string): void {
    this.password.set(value);
    this.formError.set('');
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    this.submitAttempted.set(true);
    this.formError.set('');

    if (!this.isValid()) return;

    this.isSubmitting.set(true);

    const email = this.email().trim();
    const password = this.password();

    this.api.post<LoginResponse>('/auth/login', { email, password }).subscribe({
      next: (res) => {
        this.auth.setToken(res.accessToken, res.expiresAt);
        this.isSubmitting.set(false);
        this.router.navigateByUrl('/watch-spaces');
      },
      error: (err: HttpErrorResponse) => this.handleLoginError(err),
    });
  }

  private handleLoginError(err: HttpErrorResponse): void {
    this.isSubmitting.set(false);

    if (err.status === 401) {
      this.formError.set('Invalid email or password');
      return;
    }

    this.formError.set('Something went wrong. Please try again.');
  }

  private isFieldVisible(field: string): boolean {
    return this.submitAttempted() || this.touchedFields().has(field);
  }
}
