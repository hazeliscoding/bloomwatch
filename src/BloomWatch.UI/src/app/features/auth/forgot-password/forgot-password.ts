import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BloomButtonComponent } from '../../../shared/ui/button/bloom-button';
import { BloomInputComponent } from '../../../shared/ui/input/bloom-input';
import { AuthService } from '../../../core/auth/auth.service';

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

@Component({
  selector: 'app-forgot-password',
  imports: [RouterLink, BloomButtonComponent, BloomInputComponent],
  styleUrl: './forgot-password.scss',
  template: `
    <main class="forgot-password">
      <div class="forgot-password__card">
        <div class="forgot-password__header">
          <h1 class="forgot-password__title bloom-font-display">Forgot Password?</h1>
          <p class="forgot-password__subtitle">
            Enter your email and we'll send you a link to reset your password.
          </p>
        </div>

        @if (submitted()) {
          <div class="forgot-password__confirmation" role="status">
            <p>
              If that email is registered, we've sent a reset link. Check your inbox.
            </p>
          </div>
        } @else {
          <form (submit)="onSubmit($event)" novalidate>
            <div class="forgot-password__fields">
              <bloom-input
                label="Email"
                type="email"
                [required]="true"
                autocomplete="email"
                [error]="emailError()"
                (valueChange)="onEmailChange($event)"
                (blurred)="markTouched()"
              />
            </div>

            <div class="forgot-password__submit">
              <bloom-button
                type="submit"
                variant="primary"
                [fullWidth]="true"
                [disabled]="isSubmitting()"
                [loading]="isSubmitting()"
              >
                Send Reset Link
              </bloom-button>
            </div>
          </form>
        }

        <p class="forgot-password__footer">
          <a routerLink="/login">Back to login</a>
        </p>
      </div>
    </main>
  `,
})
export class ForgotPassword {
  private readonly auth = inject(AuthService);

  readonly email = signal('');
  readonly touched = signal(false);
  readonly isSubmitting = signal(false);
  readonly submitted = signal(false);

  readonly emailError = computed(() => {
    if (!this.touched() && !this.isSubmitting()) return '';
    const value = this.email().trim();
    if (!value) return 'Email is required';
    if (!EMAIL_PATTERN.test(value)) return 'Please enter a valid email address';
    return '';
  });

  readonly isValid = computed(() => EMAIL_PATTERN.test(this.email().trim()));

  markTouched(): void {
    this.touched.set(true);
  }

  onEmailChange(value: string): void {
    this.email.set(value);
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    this.touched.set(true);

    if (!this.isValid() || this.isSubmitting()) return;

    this.isSubmitting.set(true);

    this.auth.forgotPassword(this.email().trim()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.submitted.set(true);
      },
      error: () => {
        // Always show confirmation regardless of outcome (no enumeration)
        this.isSubmitting.set(false);
        this.submitted.set(true);
      },
    });
  }
}
