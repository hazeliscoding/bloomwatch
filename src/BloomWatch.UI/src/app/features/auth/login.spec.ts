import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { Login } from './login';
import { Register } from './register';
import { AuthService } from '../../core/auth/auth.service';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let httpTesting: HttpTestingController;
  let authService: AuthService;
  let router: Router;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          { path: 'watch-spaces', component: Register },
          { path: 'register', component: Register },
        ]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
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
  // Field rendering
  // --------------------------------------------------------------------------

  describe('field rendering', () => {
    it('should render two input fields', () => {
      const inputs = fixture.nativeElement.querySelectorAll('bloom-input');
      expect(inputs.length).toBe(2);
    });

    it('should render a submit button', () => {
      const button = fixture.nativeElement.querySelector('bloom-button');
      expect(button).toBeTruthy();
      expect(button.textContent).toContain('Sign In');
    });

    it('should render a link to register page', () => {
      const link = fixture.nativeElement.querySelector('a[href="/register"]');
      expect(link).toBeTruthy();
      expect(link.textContent).toContain('Create one');
    });
  });

  // --------------------------------------------------------------------------
  // Validation — email
  // --------------------------------------------------------------------------

  describe('validation — email', () => {
    it('should not show error before interaction', () => {
      expect(component.emailError()).toBe('');
    });

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

  // --------------------------------------------------------------------------
  // Validation — password
  // --------------------------------------------------------------------------

  describe('validation — password', () => {
    it('should not show error before interaction', () => {
      expect(component.passwordError()).toBe('');
    });

    it('should show required error after blur with empty value', () => {
      component.markTouched('password');
      expect(component.passwordError()).toBe('Password is required');
    });

    it('should clear error when value is provided', () => {
      component.markTouched('password');
      component.password.set('anypassword');
      expect(component.passwordError()).toBe('');
    });
  });

  // --------------------------------------------------------------------------
  // Touched behavior
  // --------------------------------------------------------------------------

  describe('touched behavior', () => {
    it('should not show errors before submit attempt', () => {
      component.email.set('');
      component.password.set('');
      expect(component.emailError()).toBe('');
      expect(component.passwordError()).toBe('');
    });

    it('should show all errors after submit attempt', () => {
      component.onSubmit(new Event('submit'));
      expect(component.emailError()).toBe('Email is required');
      expect(component.passwordError()).toBe('Password is required');
    });
  });

  // --------------------------------------------------------------------------
  // isValid
  // --------------------------------------------------------------------------

  describe('isValid', () => {
    it('should be false with empty fields', () => {
      expect(component.isValid()).toBe(false);
    });

    it('should be false with invalid email', () => {
      component.email.set('bad');
      component.password.set('password123');
      expect(component.isValid()).toBe(false);
    });

    it('should be false with empty password', () => {
      component.email.set('user@example.com');
      component.password.set('');
      expect(component.isValid()).toBe(false);
    });

    it('should be true with valid email and password', () => {
      component.email.set('user@example.com');
      component.password.set('password123');
      expect(component.isValid()).toBe(true);
    });
  });

  // --------------------------------------------------------------------------
  // Submission flow
  // --------------------------------------------------------------------------

  function fillValidForm(): void {
    component.email.set('bloom@example.com');
    component.password.set('password123');
  }

  describe('successful login', () => {
    it('should call login endpoint with credentials', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        email: 'bloom@example.com',
        password: 'password123',
      });

      const futureDate = new Date(Date.now() + 3600_000).toISOString();
      req.flush({ accessToken: 'jwt-token', expiresAt: futureDate });

      expect(authService.token()).toBe('jwt-token');
      expect(authService.isAuthenticated()).toBe(true);
    });

    it('should navigate to /watch-spaces on success', () => {
      const navigateSpy = vi.spyOn(router, 'navigateByUrl');

      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      const futureDate = new Date(Date.now() + 3600_000).toISOString();
      req.flush({ accessToken: 'jwt-token', expiresAt: futureDate });

      expect(navigateSpy).toHaveBeenCalledWith('/watch-spaces');
    });

    it('should set isSubmitting during request', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));
      expect(component.isSubmitting()).toBe(true);

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      req.flush({ accessToken: 'jwt', expiresAt: new Date(Date.now() + 3600_000).toISOString() });

      expect(component.isSubmitting()).toBe(false);
    });

    it('should not submit when form is invalid', () => {
      component.onSubmit(new Event('submit'));
      httpTesting.expectNone(r => r.url.endsWith('/auth/login'));
      expect(component.isSubmitting()).toBe(false);
    });
  });

  // --------------------------------------------------------------------------
  // Error handling
  // --------------------------------------------------------------------------

  describe('401 error handling', () => {
    it('should show generic error for invalid credentials', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      req.flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

      expect(component.formError()).toBe('Invalid email or password');
      expect(component.isSubmitting()).toBe(false);
    });
  });

  describe('500/network error handling', () => {
    it('should show form-level error for server errors', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      req.flush(null, { status: 500, statusText: 'Internal Server Error' });

      expect(component.formError()).toBe('Something went wrong. Please try again.');
      expect(component.isSubmitting()).toBe(false);
    });

    it('should show form-level error for network errors', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      req.error(new ProgressEvent('error'));

      expect(component.formError()).toBe('Something went wrong. Please try again.');
    });
  });

  // --------------------------------------------------------------------------
  // Error clearing
  // --------------------------------------------------------------------------

  describe('error clearing', () => {
    it('should clear form error when email changes', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      req.flush({}, { status: 401, statusText: 'Unauthorized' });

      expect(component.formError()).toBe('Invalid email or password');

      component.onEmailChange('new@example.com');
      expect(component.formError()).toBe('');
    });

    it('should clear form error when password changes', () => {
      fillValidForm();
      component.onSubmit(new Event('submit'));

      const req = httpTesting.expectOne(r => r.url.endsWith('/auth/login'));
      req.flush({}, { status: 401, statusText: 'Unauthorized' });

      expect(component.formError()).toBe('Invalid email or password');

      component.onPasswordChange('newpassword');
      expect(component.formError()).toBe('');
    });
  });
});
