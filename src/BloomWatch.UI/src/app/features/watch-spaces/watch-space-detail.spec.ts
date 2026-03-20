import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { WatchSpaceDetail } from './watch-space-detail';
import { WatchSpaceDetail as WatchSpaceDetailModel, InvitationDetail } from './watch-space.model';
import { AuthService } from '../../core/auth/auth.service';

describe('WatchSpaceDetail — Invitations', () => {
  let component: WatchSpaceDetail;
  let fixture: ComponentFixture<WatchSpaceDetail>;
  let httpTesting: HttpTestingController;
  let authService: AuthService;

  const ownerUserId = 'user-owner';

  const mockDetail: WatchSpaceDetailModel = {
    watchSpaceId: 'ws-1',
    name: 'Anime Club',
    createdAt: '2026-01-15T00:00:00Z',
    members: [
      { userId: ownerUserId, displayName: 'Owner', role: 'Owner', joinedAt: '2026-01-15T00:00:00Z' },
      { userId: 'user-2', displayName: 'Member', role: 'Member', joinedAt: '2026-02-01T00:00:00Z' },
    ],
  };

  const mockInvitations: InvitationDetail[] = [
    {
      invitationId: 'inv-1',
      invitedEmail: 'friend@example.com',
      status: 'Pending',
      expiresAt: '2026-03-26T00:00:00Z',
      createdAt: '2026-03-19T00:00:00Z',
    },
  ];

  // Helper: create a fake JWT token with the given sub claim
  function fakeJwt(sub: string): string {
    const header = btoa(JSON.stringify({ alg: 'HS256' }));
    const payload = btoa(JSON.stringify({ sub }));
    return `${header}.${payload}.signature`;
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [WatchSpaceDetail],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'ws-1' } } },
        },
      ],
    });

    fixture = TestBed.createComponent(WatchSpaceDetail);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  function flushAnimeList(): void {
    const animeReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/anime') && r.method === 'GET'
    );
    animeReq.flush({ items: [] });
    fixture.detectChanges();
  }

  function initAsOwner(): void {
    // Set auth token so userId() returns ownerUserId
    authService.setToken(fakeJwt(ownerUserId), '2099-01-01T00:00:00Z');
    fixture.detectChanges();

    // Flush detail request
    const detailReq = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1') && !r.url.includes('/anime') && !r.url.includes('/invitations'));
    detailReq.flush(mockDetail);
    fixture.detectChanges();

    // Flush anime list request (from child component)
    flushAnimeList();

    // Flush invitations request (auto-loaded for owner)
    const invReq = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1/invitations'));
    invReq.flush(mockInvitations);
    fixture.detectChanges();
  }

  function initAsMember(): void {
    authService.setToken(fakeJwt('user-2'), '2099-01-01T00:00:00Z');
    fixture.detectChanges();

    const detailReq = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1') && !r.url.includes('/anime') && !r.url.includes('/invitations'));
    detailReq.flush(mockDetail);
    fixture.detectChanges();

    // Flush anime list request (from child component)
    flushAnimeList();
    // No invitations request expected for non-owner
  }

  // ---- Invite Form Visibility ----

  it('should show invite form for owner', () => {
    initAsOwner();
    expect(component.isOwner()).toBe(true);
    // Component signals are accessible
    expect(component.inviteEmail()).toBe('');
  });

  it('should not load invitations for non-owner', () => {
    initAsMember();
    expect(component.isOwner()).toBe(false);
    expect(component.invitations()).toEqual([]);
  });

  // ---- Send Invitation ----

  it('should send invitation and refresh list on success', () => {
    initAsOwner();

    component.onInviteEmailChange('new@example.com');
    component.sendInvite();
    expect(component.isInviting()).toBe(true);

    // Flush the invite POST
    const inviteReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations') && r.method === 'POST'
    );
    expect(inviteReq.request.body).toEqual({ email: 'new@example.com' });
    inviteReq.flush({
      invitationId: 'inv-2',
      invitedEmail: 'new@example.com',
      status: 'Pending',
      expiresAt: '2026-03-26T00:00:00Z',
      token: 'tok-2',
    });
    fixture.detectChanges();

    expect(component.isInviting()).toBe(false);
    expect(component.inviteSuccess()).toContain('new@example.com');
    expect(component.inviteEmail()).toBe('');

    // Flush the refresh GET
    const refreshReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations') && r.method === 'GET'
    );
    refreshReq.flush([...mockInvitations, {
      invitationId: 'inv-2',
      invitedEmail: 'new@example.com',
      status: 'Pending',
      expiresAt: '2026-03-26T00:00:00Z',
      createdAt: '2026-03-19T00:00:00Z',
    }]);
    fixture.detectChanges();

    expect(component.invitations().length).toBe(2);
  });

  it('should show error for duplicate invitation (409)', () => {
    initAsOwner();

    component.onInviteEmailChange('friend@example.com');
    component.sendInvite();

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations') && r.method === 'POST'
    );
    req.flush('Conflict', { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();

    expect(component.inviteError()).toContain('already');
    expect(component.isInviting()).toBe(false);
  });

  it('should show error for unregistered email (422)', () => {
    initAsOwner();

    component.onInviteEmailChange('unknown@example.com');
    component.sendInvite();

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations') && r.method === 'POST'
    );
    req.flush('Unprocessable', { status: 422, statusText: 'Unprocessable Entity' });
    fixture.detectChanges();

    expect(component.inviteError()).toContain('not a registered');
    expect(component.isInviting()).toBe(false);
  });

  // ---- Invitations List ----

  it('should load invitations list for owner', () => {
    initAsOwner();
    expect(component.invitations().length).toBe(1);
    expect(component.invitations()[0].invitedEmail).toBe('friend@example.com');
  });

  it('should show empty state when no invitations', () => {
    authService.setToken(fakeJwt(ownerUserId), '2099-01-01T00:00:00Z');
    fixture.detectChanges();

    const detailReq = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1') && !r.url.includes('/anime') && !r.url.includes('/invitations'));
    detailReq.flush(mockDetail);
    fixture.detectChanges();

    flushAnimeList();

    const invReq = httpTesting.expectOne((r) => r.url.includes('/watchspaces/ws-1/invitations'));
    invReq.flush([]);
    fixture.detectChanges();

    expect(component.invitations()).toEqual([]);
  });

  // ---- Revoke Invitation ----

  it('should revoke invitation after confirmation', () => {
    initAsOwner();
    vi.spyOn(globalThis, 'confirm').mockReturnValue(true);

    component.revokeInvitation('inv-1', 'friend@example.com');

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations/inv-1') && r.method === 'DELETE'
    );
    req.flush(null);
    fixture.detectChanges();

    expect(component.invitations().length).toBe(0);
  });

  it('should not revoke if confirmation is cancelled', () => {
    initAsOwner();
    vi.spyOn(globalThis, 'confirm').mockReturnValue(false);

    component.revokeInvitation('inv-1', 'friend@example.com');
    // No HTTP request should be made
    expect(component.invitations().length).toBe(1);
  });
});
