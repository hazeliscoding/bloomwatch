import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { AuthService } from '../auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    localStorage.clear();
  });

  function runGuard() {
    return TestBed.runInInjectionContext(() => authGuard({} as any, {} as any)) as boolean | UrlTree;
  }

  it('should allow navigation when authenticated', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    authService.setToken('valid-jwt', futureDate);

    expect(runGuard()).toBe(true);
  });

  it('should redirect to /login when unauthenticated', () => {
    const result = runGuard();

    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/login');
  });

  it('should redirect to /login when token is expired', () => {
    const pastDate = new Date(Date.now() - 1000).toISOString();
    authService.setToken('expired-jwt', pastDate);

    const result = runGuard();

    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/login');
  });
});
