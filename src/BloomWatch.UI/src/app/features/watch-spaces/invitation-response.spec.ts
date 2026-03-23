import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { InvitationResponse } from './invitation-response';
import { InvitationPreview } from './watch-space.model';

describe('InvitationResponse', () => {
  let component: InvitationResponse;
  let fixture: ComponentFixture<InvitationResponse>;
  let httpTesting: HttpTestingController;
  let router: Router;

  const mockPreview: InvitationPreview = {
    watchSpaceId: 'ws-1',
    watchSpaceName: 'Anime Club',
    invitedEmail: 'friend@example.com',
    status: 'Pending',
    expiresAt: '2026-03-26T00:00:00Z',
  };

  function setup(token: string = 'valid-token') {
    TestBed.configureTestingModule({
      imports: [InvitationResponse],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => token } } },
        },
      ],
    });

    fixture = TestBed.createComponent(InvitationResponse);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  }

  afterEach(() => {
    httpTesting.verify();
  });

  function flushPreview(preview: InvitationPreview = mockPreview): void {
    const req = httpTesting.expectOne((r) => r.url.includes('/watchspaces/invitations/'));
    req.flush(preview);
    fixture.detectChanges();
  }

  function flushPreviewError(status: number): void {
    const req = httpTesting.expectOne((r) => r.url.includes('/watchspaces/invitations/'));
    req.flush('Error', { status, statusText: 'Error' });
    fixture.detectChanges();
  }

  // ---- Loading State ----

  it('should show loading state initially', () => {
    setup();
    fixture.detectChanges();
    expect(component.state()).toBe('loading');
    httpTesting.expectOne((r) => r.url.includes('/watchspaces/invitations/')).flush(mockPreview);
  });

  // ---- Successful Preview Load ----

  it('should load invitation preview and show ready state', () => {
    setup();
    fixture.detectChanges();
    flushPreview();
    expect(component.state()).toBe('ready');
    expect(component.preview()?.watchSpaceName).toBe('Anime Club');
  });

  // ---- Accept Flow ----

  it('should accept invitation and show accepted state', () => {
    setup();
    fixture.detectChanges();
    flushPreview();

    component.accept();
    expect(component.state()).toBe('accepting');

    const req = httpTesting.expectOne((r) => r.url.includes('/accept'));
    expect(req.request.method).toBe('POST');
    req.flush({ watchSpaceId: 'ws-1' });
    fixture.detectChanges();

    expect(component.state()).toBe('accepted');
    expect(component.acceptedWatchSpaceId()).toBe('ws-1');
  });

  it('should navigate to watch space from accepted state', () => {
    setup();
    vi.spyOn(router, 'navigate');
    fixture.detectChanges();
    flushPreview();

    component.accept();
    const req = httpTesting.expectOne((r) => r.url.includes('/accept'));
    req.flush({ watchSpaceId: 'ws-1' });
    fixture.detectChanges();

    component.goToWatchSpace();
    expect(router.navigate).toHaveBeenCalledWith(['/watch-spaces', 'ws-1']);
  });

  // ---- Decline Flow ----

  it('should decline invitation and show declined state', () => {
    setup();
    fixture.detectChanges();
    flushPreview();

    component.decline();
    expect(component.state()).toBe('declining');

    const req = httpTesting.expectOne((r) => r.url.includes('/decline'));
    expect(req.request.method).toBe('POST');
    req.flush(null);
    fixture.detectChanges();

    expect(component.state()).toBe('declined');
  });

  // ---- Error States ----

  it('should show expired state for expired invitation (410)', () => {
    setup();
    fixture.detectChanges();
    flushPreviewError(410);
    expect(component.state()).toBe('expired');
  });

  it('should show error for wrong account (403)', () => {
    setup();
    fixture.detectChanges();
    flushPreviewError(403);
    expect(component.state()).toBe('error');
    expect(component.errorMessage()).toContain('different account');
  });

  it('should show error for not found (404)', () => {
    setup();
    fixture.detectChanges();
    flushPreviewError(404);
    expect(component.state()).toBe('error');
    expect(component.errorMessage()).toContain('not found');
  });

  it('should show already-used state for used invitation', () => {
    setup();
    fixture.detectChanges();
    flushPreview({ ...mockPreview, status: 'Accepted' });
    expect(component.state()).toBe('already-used');
  });

  // ---- Accept Error ----

  it('should show expired state when accept fails with 410', () => {
    setup();
    fixture.detectChanges();
    flushPreview();

    component.accept();
    const req = httpTesting.expectOne((r) => r.url.includes('/accept'));
    req.flush('Error', { status: 410, statusText: 'Gone' });
    fixture.detectChanges();

    expect(component.state()).toBe('expired');
  });
});
