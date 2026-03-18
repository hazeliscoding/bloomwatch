import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomInputComponent } from '../../shared/ui/input/bloom-input';
import { ApiService } from '../../core/http/api.service';
import { AuthService } from '../../core/auth/auth.service';

interface RegisterResponse {
  id: string;
  email: string;
  displayName: string;
}

interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

@Component({
  selector: 'app-register',
  imports: [RouterLink, BloomButtonComponent, BloomInputComponent],
  styleUrl: './register.scss',
  template: `
    <main class="register">
      <div class="register__card">
        <div class="register__header">
          <h1 class="register__title bloom-font-display">Create Account</h1>
          <p class="register__subtitle">Join BloomWatch and start tracking anime together.</p>
        </div>

        @if (formError()) {
          <div class="register__error-banner" role="alert">
            {{ formError() }}
          </div>
        }

        <form (submit)="onSubmit($event)" novalidate>
          <div class="register__fields">
            <bloom-input
              label="Display Name"
              [required]="true"
              autocomplete="name"
              [error]="displayNameError()"
              (valueChange)="displayName.set($event)"
              (blurred)="markTouched('displayName')"
            />

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
              autocomplete="new-password"
              [error]="passwordError()"
              (valueChange)="password.set($event)"
              (blurred)="markTouched('password')"
            />

            <bloom-input
              label="Confirm Password"
              type="password"
              [required]="true"
              autocomplete="new-password"
              [error]="confirmPasswordError()"
              (valueChange)="confirmPassword.set($event)"
              (blurred)="markTouched('confirmPassword')"
            />
          </div>

          <div class="register__submit">
            <bloom-button
              type="submit"
              variant="primary"
              [fullWidth]="true"
              [disabled]="!isValid() || isSubmitting()"
              [loading]="isSubmitting()"
            >
              Create Account
            </bloom-button>
          </div>
        </form>

        <p class="register__footer">
          Already have an account? <a routerLink="/login">Sign in</a>
        </p>
      </div>
    </main>
  `,
})
export class Register {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  // Form field signals
  readonly displayName = signal('');
  readonly email = signal('');
  readonly password = signal('');
  readonly confirmPassword = signal('');

  // Touched tracking
  readonly touchedFields = signal(new Set<string>());
  readonly submitAttempted = signal(false);

  // Submission state
  readonly isSubmitting = signal(false);
  readonly formError = signal('');
  readonly serverEmailError = signal('');

  // Validation: display name
  readonly displayNameError = computed(() => {
    if (!this.isFieldVisible('displayName')) return '';
    const value = this.displayName().trim();
    if (!value) return 'Display name is required';
    return '';
  });

  // Validation: email
  readonly emailError = computed(() => {
    if (!this.isFieldVisible('email')) return '';
    const serverErr = this.serverEmailError();
    if (serverErr) return serverErr;
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
    if (value.length < 8) return 'Password must be at least 8 characters';
    return '';
  });

  // Validation: confirm password
  readonly confirmPasswordError = computed(() => {
    if (!this.isFieldVisible('confirmPassword')) return '';
    const value = this.confirmPassword();
    if (!value) return 'Confirm password is required';
    if (value !== this.password()) return 'Passwords do not match';
    return '';
  });

  // Overall validity (checks raw values, not display-gated errors)
  readonly isValid = computed(() => {
    const dn = this.displayName().trim();
    const em = this.email().trim();
    const pw = this.password();
    const cpw = this.confirmPassword();
    return (
      dn.length > 0 &&
      em.length > 0 &&
      EMAIL_PATTERN.test(em) &&
      pw.length >= 8 &&
      cpw === pw
    );
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
    this.serverEmailError.set('');
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    this.submitAttempted.set(true);
    this.formError.set('');
    this.serverEmailError.set('');

    if (!this.isValid()) return;

    this.isSubmitting.set(true);

    const email = this.email().trim();
    const password = this.password();
    const displayName = this.displayName().trim();

    this.api.post<RegisterResponse>('/auth/register', { email, password, displayName }).subscribe({
      next: () => this.autoLogin(email, password),
      error: (err: HttpErrorResponse) => this.handleRegisterError(err),
    });
  }

  private autoLogin(email: string, password: string): void {
    this.api.post<LoginResponse>('/auth/login', { email, password }).subscribe({
      next: (res) => {
        this.auth.setToken(res.accessToken, res.expiresAt);
        this.isSubmitting.set(false);
        this.router.navigateByUrl('/watch-spaces');
      },
      error: () => {
        this.isSubmitting.set(false);
        this.formError.set('Account created! Please log in.');
        this.router.navigateByUrl('/login');
      },
    });
  }

  private handleRegisterError(err: HttpErrorResponse): void {
    this.isSubmitting.set(false);

    if (err.status === 409) {
      this.serverEmailError.set('This email is already registered');
      return;
    }

    if (err.status === 400 && err.error?.errors) {
      this.mapServerValidationErrors(err.error.errors);
      return;
    }

    this.formError.set('Something went wrong. Please try again.');
  }

  private mapServerValidationErrors(errors: Record<string, string[]>): void {
    for (const [field, messages] of Object.entries(errors)) {
      const message = messages[0];
      if (!message) continue;
      const key = field.toLowerCase();
      if (key === 'email') {
        this.serverEmailError.set(message);
      } else if (key === 'password') {
        this.formError.set(message);
      } else if (key === 'displayname') {
        this.formError.set(message);
      }
    }
  }

  private isFieldVisible(field: string): boolean {
    return this.submitAttempted() || this.touchedFields().has(field);
  }
}
