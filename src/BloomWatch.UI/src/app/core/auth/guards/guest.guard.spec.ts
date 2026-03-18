import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { AuthService } from '../auth.service';
import { guestGuard } from './guest.guard';

describe('guestGuard', () => {
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
    return TestBed.runInInjectionContext(() => guestGuard({} as any, {} as any)) as boolean | UrlTree;
  }

  it('should allow navigation when unauthenticated', () => {
    expect(runGuard()).toBe(true);
  });

  it('should redirect to /watch-spaces when authenticated', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    authService.setToken('valid-jwt', futureDate);

    const result = runGuard();

    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/watch-spaces');
  });
});
