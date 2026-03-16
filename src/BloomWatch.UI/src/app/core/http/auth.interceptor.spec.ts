import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  it('should attach Bearer token when authenticated', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    authService.setToken('my-jwt', futureDate);

    http.get('/api/test').subscribe();

    const req = httpTesting.expectOne('/api/test');
    expect(req.request.headers.get('Authorization')).toBe('Bearer my-jwt');
    req.flush({});
  });

  it('should not attach Authorization header when unauthenticated', () => {
    http.get('/api/test').subscribe();

    const req = httpTesting.expectOne('/api/test');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should skip token for /auth/ endpoints', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    authService.setToken('my-jwt', futureDate);

    http.post('/auth/login', {}).subscribe();

    const req = httpTesting.expectOne('/auth/login');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should skip token for /auth/register endpoint', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    authService.setToken('my-jwt', futureDate);

    http.post('/auth/register', {}).subscribe();

    const req = httpTesting.expectOne('/auth/register');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should clear token and redirect on 401 response', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    authService.setToken('my-jwt', futureDate);
    vi.spyOn(router, 'navigateByUrl');
    vi.spyOn(authService, 'clearToken');

    http.get('/api/test').subscribe({ error: () => {} });

    const req = httpTesting.expectOne('/api/test');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(authService.clearToken).toHaveBeenCalled();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
  });

  it('should not redirect on non-401 errors', () => {
    vi.spyOn(router, 'navigateByUrl');
    vi.spyOn(authService, 'clearToken');

    http.get('/api/test').subscribe({ error: () => {} });

    const req = httpTesting.expectOne('/api/test');
    req.flush('Not Found', { status: 404, statusText: 'Not Found' });

    expect(authService.clearToken).not.toHaveBeenCalled();
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });
});
