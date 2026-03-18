import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { Register } from './register';
import { AuthService } from '../../core/auth/auth.service';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
  let httpTesting: HttpTestingController;
  let authService: AuthService;
  let router: Router;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          { path: 'watch-spaces', component: Register },
          { path: 'login', component: Register },
        ]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  // --------------------------------------------------------------------------
  // 6.1 — Field rendering & validation
  // --------------------------------------------------------------------------

  describe('field rendering', () => {
    it('should render four input fields', () => {
      const inputs = fixture.nativeElement.querySelectorAll('bloom-input');
      expect(inputs.length).toBe(4);
    });

    it('should render a submit button', () => {
      const button = fixture.nativeElement.querySelector('bloom-button');
      expect(button).toBeTruthy();
      expect(button.textContent).toContain('Create Account');
    });

    it('should render a link to login page', () => {
      const link = fixture.nativeElement.querySelector('a[href="/login"]');
      expect(link).toBeTruthy();
      expect(link.textContent).toContain('Sign in');
    });
  });

  describe('validation — display name', () => {
    it('should not show error before interaction', () => {
      expect(component.displayNameError()).toBe('');
    });

    it('should show required error after blur with empty value', () => {
      component.markTouched('displayName');
      expect(component.displayNameError()).toBe('Display name is required');
    });

    it('should clear error when value is provided', () => {
      component.markTouched('displayName');
      component.displayName.set('Bloom');
      expect(component.displayNameError()).toBe('');
    });
  });

  describe('validation — email', () => {
    it('should show required error after blur with empty value', () => {
      component.markTouched('email');
      expect(component.emailError()).toBe('Email is required');
    });

    it('should show format error for invalid email', () => {
      component.markTouched('email');
      component.email.set('notanemail');
      expect(component.emailError()).toBe('Please enter a valid email address');
    });

    it('should clear error for valid email', () => {
      component.markTouched('email');
      component.email.set('user@example.com');
      expect(component.emailError()).toBe('');
    });
  });

  describe('validation — password', () => {
    it('should show required error after blur with empty value', () => {
      component.markTouched('password');
      expect(component.passwordError()).toBe('Password is required');
    });

    it('should show min length error for short password', () => {
      component.markTouched('password');
      component.password.set('short');
      expect(component.passwordError()).toBe('Password must be at least 8 characters');
    });

    it('should clear error for valid password', () => {
      component.markTouched('password');
      component.password.set('longpassword');
      expect(component.passwordError()).toBe('');
    });
  });

  describe('validation — confirm password', () => {
    it('should show required error after blur with empty value', () => {
      component.markTouched('confirmPassword');
      expect(component.confirmPasswordError()).toBe('Confirm password is required');
    });

    it('should show mismatch error when passwords differ', () => {
      component.markTouched('confirmPassword');
      component.password.set('password123');
      component.confirmPassword.set('different');
      expect(component.confirmPasswordError()).toBe('Passwords do not match');
    });

    it('should clear error when passwords match', () => {
      component.markTouched('confirmPassword');
      component.password.set('password123');
      component.confirmPassword.set('password123');
      expect(component.confirmPasswordError()).toBe('');
    });
  });

  describe('touched behavior', () => {
    it('should not show errors before submit attempt', () => {
      component.displayName.set('');
      component.email.set('');
      component.password.set('');
      component.confirmPassword.set('');
      expect(component.displayNameError()).toBe('');
      expect(component.emailError()).toBe('');
      expect(component.passwordError()).toBe('');
      expect(component.confirmPasswordError()).toBe('');
    });

    it('should show all errors after submit attempt', () => {
      component.onSubmit(new Event('submit'));
      expect(component.displayNameError()).toBe('Display name is required');
      expect(component.emailError()).toBe('Email is required');
      expect(component.passwordError()).toBe('Password is required');
      expect(component.confirmPasswordError()).toBe('Confirm password is required');
    });
  });

  describe('isValid', () => {
    it('should be false with empty fields', () => {
      expect(component.isValid()).toBe(false);
    });

    it('should be false with invalid email', () => {
      component.displayName.set('User');
      component.email.set('bad');
      component.password.set('password123');
      component.confirmPassword.set('password123');
      expect(component.isValid()).toBe(false);
    });

    it('should be false with short password', () => {
      component.displayName.set('User');
      component.email.set('user@example.com');
      component.password.set('short');
      component.confirmPassword.set('short');
      expect(component.isValid()).toBe(false);
    });

    it('should be false with mismatched passwords', () => {
      component.displayName.set('User');
      component.email.set('user@example.com');
      component.password.set('password123');
      component.confirmPassword.set('different');
      expect(component.isValid()).toBe(false);
    });

    it('should be true with all valid fields', () => {
      component.displayName.set('User');
      component.email.set('user@example.com');
      component.password.set('password123');
      component.confirmPassword.set('password123');
      expect(component.isValid()).toBe(true);
    });
  });

  // --------------------------------------------------------------------------
  // 6.2 — Submission flow
  // --------------------------------------------------------------------------

  function fillValidForm(): void {
    component.displayName.set('Bloom User');
    component.email.set('bloom@example.com');
    component.password.set('password123');
    component.confirmPassword.set('password123');
  }

  describe('successful registration + auto-login', () => {
    it('should call register and then login endpoints', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const registerReq = httpTesting.expectOne(r => r.url.endsWith('/auth/register'));
      expect(registerReq.request.method).toBe('POST');
      expect(registerReq.request.body).toEqual({
        email: 'bloom@example.com',
        password: 'password123',
        displayName: 'Bloom User',
      });
      registerReq.flush({ id: '1', email: 'bloom@example.com', displayName: 'Bloom User' }, { status: 201, statusText: 'Created' });

      const loginReq = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      expect(loginReq.request.method).toBe('POST');
      expect(loginReq.request.body).toEqual({
        email: 'bloom@example.com',
        password: 'password123',
      });

      const futureDate = new Date(Date.now() + 3600_000).toISOString();
      loginReq.flush({ accessToken: 'jwt-token', expiresAt: futureDate });

      expect(authService.token()).toBe('jwt-token');
      expect(authService.isAuthenticated()).toBe(true);
    });

    it('should set isSubmitting during request', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));
      expect(component.isSubmitting()).toBe(true);

      const registerReq = httpTesting.expectOne(r => r.url.endsWith('/auth/register'));
      registerReq.flush({ id: '1', email: 'bloom@example.com', displayName: 'Bloom User' });

      // Still submitting during login
      expect(component.isSubmitting()).toBe(true);

      const loginReq = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      loginReq.flush({ accessToken: 'jwt', expiresAt: new Date(Date.now() + 3600_000).toISOString() });

      expect(component.isSubmitting()).toBe(false);
    });

    it('should not submit when form is invalid', () => {
      component.onSubmit(new Event('submit'));
      httpTesting.expectNone(r => r.url.endsWith('/auth/register'));
      expect(component.isSubmitting()).toBe(false);
    });
  });

  describe('409 error handling', () => {
    it('should show email already registered error', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/register'));
      req.flush({ message: 'Email already in use' }, { status: 409, statusText: 'Conflict' });

      expect(component.serverEmailError()).toBe('This email is already registered');
      expect(component.emailError()).toBe('This email is already registered');
      expect(component.isSubmitting()).toBe(false);
    });

    it('should clear server email error when user edits email', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/register'));
      req.flush({}, { status: 409, statusText: 'Conflict' });

      expect(component.serverEmailError()).toBe('This email is already registered');

      component.onEmailChange('newemail@example.com');
      expect(component.serverEmailError()).toBe('');
    });
  });

  describe('400 error handling', () => {
    it('should map server validation errors to fields', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/register'));
      req.flush(
        { errors: { Email: ['Email format is invalid'] } },
        { status: 400, statusText: 'Bad Request' },
      );

      expect(component.serverEmailError()).toBe('Email format is invalid');
      expect(component.isSubmitting()).toBe(false);
    });
  });

  describe('500/network error handling', () => {
    it('should show form-level error for server errors', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/register'));
      req.flush(null, { status: 500, statusText: 'Internal Server Error' });

      expect(component.formError()).toBe('Something went wrong. Please try again.');
      expect(component.isSubmitting()).toBe(false);
    });

    it('should show form-level error for network errors', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/register'));
      req.error(new ProgressEvent('error'));

      expect(component.formError()).toBe('Something went wrong. Please try again.');
    });
  });

  describe('auto-login failure fallback', () => {
    it('should redirect to login on auto-login failure', () => {
      const navigateSpy = vi.spyOn(router, 'navigateByUrl');

      fillValidForm();
      component.onSubmit(new Event('submit'));

      const registerReq = httpTesting.expectOne(r => r.url.endsWith('/auth/register'));
      registerReq.flush({ id: '1', email: 'bloom@example.com', displayName: 'Bloom User' });

      const loginReq = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      loginReq.flush(null, { status: 500, statusText: 'Internal Server Error' });

      expect(component.formError()).toBe('Account created! Please log in.');
      expect(navigateSpy).toHaveBeenCalledWith('/login');
      expect(component.isSubmitting()).toBe(false);
    });
  });
});
