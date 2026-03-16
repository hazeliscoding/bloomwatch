import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should start unauthenticated when no token in storage', () => {
    expect(service.isAuthenticated()).toBe(false);
    expect(service.token()).toBeNull();
  });

  it('should store token and expiry on setToken', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    service.setToken('test-jwt', futureDate);

    expect(localStorage.getItem('bloom_access_token')).toBe('test-jwt');
    expect(localStorage.getItem('bloom_token_expires_at')).toBe(futureDate);
  });

  it('should expose token via signal after setToken', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    service.setToken('test-jwt', futureDate);

    expect(service.token()).toBe('test-jwt');
  });

  it('should be authenticated with a valid non-expired token', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    service.setToken('test-jwt', futureDate);

    expect(service.isAuthenticated()).toBe(true);
  });

  it('should not be authenticated with an expired token', () => {
    const pastDate = new Date(Date.now() - 1000).toISOString();
    service.setToken('expired-jwt', pastDate);

    expect(service.isAuthenticated()).toBe(false);
  });

  it('should clear token and expiry on clearToken', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    service.setToken('test-jwt', futureDate);
    service.clearToken();

    expect(service.token()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(localStorage.getItem('bloom_access_token')).toBeNull();
    expect(localStorage.getItem('bloom_token_expires_at')).toBeNull();
  });

  it('should initialize from localStorage on construction', () => {
    const futureDate = new Date(Date.now() + 3600_000).toISOString();
    localStorage.setItem('bloom_access_token', 'persisted-jwt');
    localStorage.setItem('bloom_token_expires_at', futureDate);

    // Create a new instance to test constructor initialization
    const freshService = TestBed.inject(AuthService);

    // Same singleton, so we need a fresh injector
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
    const newService = TestBed.inject(AuthService);

    expect(newService.token()).toBe('persisted-jwt');
    expect(newService.isAuthenticated()).toBe(true);
  });
});
