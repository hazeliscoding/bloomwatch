import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ApiService } from './api.service';

describe('ApiService', () => {
  let service: ApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should prefix GET requests with apiBaseUrl', () => {
    service.get('/users/me').subscribe();

    const req = httpTesting.expectOne((r) => r.url.endsWith('/users/me'));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should prefix POST requests with apiBaseUrl', () => {
    const body = { email: 'test@example.com', password: 'pass' };
    service.post('/auth/login', body).subscribe();

    const req = httpTesting.expectOne((r) => r.url.endsWith('/auth/login'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush({});
  });

  it('should prefix PUT requests with apiBaseUrl', () => {
    const body = { name: 'updated' };
    service.put('/watchspaces/1', body).subscribe();

    const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/1'));
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('should prefix PATCH requests with apiBaseUrl', () => {
    const body = { name: 'patched' };
    service.patch('/watchspaces/1', body).subscribe();

    const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/1'));
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should prefix DELETE requests with apiBaseUrl', () => {
    service.delete('/watchspaces/1').subscribe();

    const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/1'));
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('should return typed response', () => {
    interface User {
      id: string;
      email: string;
    }

    let result: User | undefined;
    service.get<User>('/users/me').subscribe((r) => (result = r));

    const req = httpTesting.expectOne((r) => r.url.endsWith('/users/me'));
    req.flush({ id: '123', email: 'test@example.com' });

    expect(result).toEqual({ id: '123', email: 'test@example.com' });
  });
});
