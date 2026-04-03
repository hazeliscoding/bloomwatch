import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BloomButtonComponent } from '../../../shared/ui/button/bloom-button';
import { BloomInputComponent } from '../../../shared/ui/input/bloom-input';
import { AuthService } from '../../../core/auth/auth.service';

const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;

@Component({
  selector: 'app-reset-password',
  imports: [RouterLink, BloomButtonComponent, BloomInputComponent],
  styleUrl: './reset-password.scss',
  template: `
    <main class="reset-password">
      <div class="reset-password__card">
        <div class="reset-password__header">
          <h1 class="reset-password__title bloom-font-display">Set New Password</h1>
        </div>

        @if (!token()) {
          <div class="reset-password__error-banner" role="alert">
            Invalid or missing reset link. Please request a new one.
          </div>
          <p class="reset-password__footer">
            <a routerLink="/forgot-password">Request a new reset link</a>
          </p>
        } @else if (succeeded()) {
          <div class="reset-password__success" role="status">
            <p>Your password has been reset!</p>
          </div>
          <p class="reset-password__footer">
            <a routerLink="/login">Go to login</a>
          </p>
        } @else {
          @if (apiError()) {
            <div class="reset-password__error-banner" role="alert">
              {{ apiError() }}
            </div>
          }

          <form (submit)="onSubmit($event)" novalidate>
            <div class="reset-password__fields">
              <bloom-input
                label="New Password"
                type="password"
                [required]="true"
                autocomplete="new-password"
                [error]="newPasswordError()"
                (valueChange)="newPassword.set($event)"
                (blurred)="markTouched('newPassword')"
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

            <div class="reset-password__submit">
              <bloom-button
                type="submit"
                variant="primary"
                [fullWidth]="true"
                [disabled]="isSubmitting()"
                [loading]="isSubmitting()"
              >
                Reset Password
              </bloom-button>
            </div>
          </form>

          <p class="reset-password__footer">
            <a routerLink="/login">Back to login</a>
          </p>
        }
      </div>
    </main>
  `,
})
export class ResetPassword implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly token = signal<string | null>(null);
  readonly newPassword = signal('');
  readonly confirmPassword = signal('');
  readonly touchedFields = signal(new Set<string>());
  readonly submitAttempted = signal(false);
  readonly isSubmitting = signal(false);
  readonly succeeded = signal(false);
  readonly apiError = signal('');

  ngOnInit(): void {
    const tokenParam = this.route.snapshot.queryParamMap.get('token');
    this.token.set(tokenParam);
  }

  readonly newPasswordError = computed(() => {
    if (!this.isFieldVisible('newPassword')) return '';
    const value = this.newPassword();
    if (!value) return 'Password is required';
    if (!PASSWORD_PATTERN.test(value))
      return 'Must be 8+ characters with uppercase, lowercase, and a digit';
    return '';
  });

  readonly confirmPasswordError = computed(() => {
    if (!this.isFieldVisible('confirmPassword')) return '';
    if (!this.confirmPassword()) return 'Please confirm your password';
    if (this.newPassword() !== this.confirmPassword()) return 'Passwords do not match';
    return '';
  });

  readonly isValid = computed(
    () =>
      PASSWORD_PATTERN.test(this.newPassword()) &&
      this.newPassword() === this.confirmPassword(),
  );

  markTouched(field: string): void {
    this.touchedFields.update(set => {
      const next = new Set(set);
      next.add(field);
      return next;
    });
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    this.submitAttempted.set(true);
    this.apiError.set('');

    if (!this.isValid() || this.isSubmitting()) return;

    const tok = this.token();
    if (!tok) return;

    this.isSubmitting.set(true);

    this.auth.resetPassword(tok, this.newPassword()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.succeeded.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const message = err.error?.error ?? 'Something went wrong. Please try again.';
        this.apiError.set(message);
      },
    });
  }

  private isFieldVisible(field: string): boolean {
    return this.submitAttempted() || this.touchedFields().has(field);
  }
}
