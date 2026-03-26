import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { WatchSpaceSettings } from './watch-space-settings';
import { WatchSpaceDetail, InvitationDetail } from './watch-space.model';
import { AuthService } from '../../core/auth/auth.service';

describe('WatchSpaceSettings', () => {
  let component: WatchSpaceSettings;
  let fixture: ComponentFixture<WatchSpaceSettings>;
  let httpTesting: HttpTestingController;
  let authService: AuthService;

  const ownerUserId = 'user-owner';

  const mockDetail: WatchSpaceDetail = {
    watchSpaceId: 'ws-1',
    name: 'Anime Night',
    createdAt: '2026-01-15T00:00:00Z',
    members: [
      { userId: ownerUserId, displayName: 'hazel', role: 'Owner', joinedAt: '2026-03-13T00:00:00Z' },
      { userId: 'user-2', displayName: 'sakura', role: 'Member', joinedAt: '2026-03-14T00:00:00Z' },
    ],
  };

  const mockInvitations: InvitationDetail[] = [
    {
      invitationId: 'inv-1',
      invitedEmail: 'friend@email.com',
      status: 'Pending',
      expiresAt: '2026-03-23T00:00:00Z',
      createdAt: '2026-03-16T00:00:00Z',
    },
  ];

  function fakeJwt(sub: string): string {
    const header = btoa(JSON.stringify({ alg: 'HS256' }));
    const payload = btoa(JSON.stringify({ sub }));
    return `${header}.${payload}.signature`;
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [WatchSpaceSettings],
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

    fixture = TestBed.createComponent(WatchSpaceSettings);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  function initAsOwner(): void {
    authService.setToken(fakeJwt(ownerUserId), '2099-01-01T00:00:00Z');
    fixture.detectChanges();

    const detailReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1') && !r.url.includes('/invitations')
    );
    detailReq.flush(mockDetail);
    fixture.detectChanges();

    const invReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations')
    );
    invReq.flush(mockInvitations);
    fixture.detectChanges();
  }

  function initAsMember(): void {
    authService.setToken(fakeJwt('user-2'), '2099-01-01T00:00:00Z');
    fixture.detectChanges();

    const detailReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1') && !r.url.includes('/invitations')
    );
    detailReq.flush(mockDetail);
    fixture.detectChanges();
  }

  // --- Page load ---

  it('should render settings card title', () => {
    initAsOwner();
    const title = fixture.nativeElement.querySelector('.ws-settings__title');
    expect(title?.textContent).toContain('Watch Space Settings');
  });

  it('should show loading state initially', () => {
    authService.setToken(fakeJwt(ownerUserId), '2099-01-01T00:00:00Z');
    fixture.detectChanges();

    const loading = fixture.nativeElement.querySelector('.ws-settings__loading');
    expect(loading).toBeTruthy();

    const detailReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1') && !r.url.includes('/invitations')
    );
    detailReq.flush(mockDetail);
    fixture.detectChanges();
    const invReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations')
    );
    invReq.flush([]);
    fixture.detectChanges();
  });

  it('should show error on load failure', () => {
    authService.setToken(fakeJwt(ownerUserId), '2099-01-01T00:00:00Z');
    fixture.detectChanges();

    const detailReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1') && !r.url.includes('/invitations')
    );
    detailReq.flush('Error', { status: 500, statusText: 'Internal Server Error' });
    fixture.detectChanges();

    const error = fixture.nativeElement.querySelector('.ws-settings__error');
    expect(error).toBeTruthy();
  });

  // --- Space Name ---

  it('should display space name in inline edit', () => {
    initAsOwner();
    const inlineEdit = fixture.nativeElement.querySelector('.ws-settings__inline-edit');
    expect(inlineEdit?.textContent).toContain('Anime Night');
  });

  it('should show pencil icon for owner', () => {
    initAsOwner();
    const editIcon = fixture.nativeElement.querySelector('.ws-settings__edit-icon');
    expect(editIcon).toBeTruthy();
  });

  it('should not show pencil icon for non-owner', () => {
    initAsMember();
    const editIcon = fixture.nativeElement.querySelector('.ws-settings__edit-icon');
    expect(editIcon).toBeFalsy();
  });

  it('should enter edit mode when pencil is clicked', () => {
    initAsOwner();
    const editIcon = fixture.nativeElement.querySelector('.ws-settings__edit-icon') as HTMLButtonElement;
    editIcon.click();
    fixture.detectChanges();

    const renameForm = fixture.nativeElement.querySelector('.ws-settings__rename-form');
    expect(renameForm).toBeTruthy();
    expect(component.editName()).toBe('Anime Night');
  });

  it('should rename on save', () => {
    initAsOwner();
    component.startEditing();
    component.onEditNameChange('New Name');
    component.saveRename();

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1') && r.method === 'PATCH'
    );
    expect(req.request.body).toEqual({ name: 'New Name' });
    req.flush(null);
    fixture.detectChanges();

    expect(component.isEditing()).toBe(false);
    expect(component.detail()?.name).toBe('New Name');
  });

  // --- Members ---

  it('should display all members', () => {
    initAsOwner();
    const memberRows = fixture.nativeElement.querySelectorAll('.ws-settings__member-row');
    expect(memberRows.length).toBe(2);
  });

  it('should show Transfer and Remove for non-self members when owner', () => {
    initAsOwner();
    const memberRows = fixture.nativeElement.querySelectorAll('.ws-settings__member-row');
    const secondRow = memberRows[1];
    const actions = secondRow.querySelector('.ws-settings__member-actions');
    expect(actions).toBeTruthy();
    expect(actions.textContent).toContain('Transfer');
    expect(actions.textContent).toContain('Remove');
  });

  it('should not show actions for self (owner row)', () => {
    initAsOwner();
    const memberRows = fixture.nativeElement.querySelectorAll('.ws-settings__member-row');
    const ownerRow = memberRows[0];
    const actions = ownerRow.querySelector('.ws-settings__member-actions');
    expect(actions).toBeFalsy();
  });

  it('should not show member actions for non-owner', () => {
    initAsMember();
    const actions = fixture.nativeElement.querySelectorAll('.ws-settings__member-actions');
    expect(actions.length).toBe(0);
  });

  it('should remove member after confirmation', () => {
    initAsOwner();
    vi.spyOn(globalThis, 'confirm').mockReturnValue(true);

    component.removeMember('user-2', 'sakura');

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/members/user-2') && r.method === 'DELETE'
    );
    req.flush(null);
    fixture.detectChanges();

    expect(component.detail()?.members.length).toBe(1);
  });

  it('should transfer ownership after confirmation', () => {
    initAsOwner();
    vi.spyOn(globalThis, 'confirm').mockReturnValue(true);

    component.transferOwnership('user-2', 'sakura');

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/transfer-ownership') && r.method === 'POST'
    );
    expect(req.request.body).toEqual({ newOwnerId: 'user-2' });
    req.flush(null);
    fixture.detectChanges();

    const members = component.detail()!.members;
    expect(members.find(m => m.userId === 'user-2')?.role).toBe('Owner');
    expect(members.find(m => m.userId === ownerUserId)?.role).toBe('Member');
  });

  // --- Invitations (owner) ---

  it('should show invite form for owner', () => {
    initAsOwner();
    const inviteRow = fixture.nativeElement.querySelector('.ws-settings__invite-row');
    expect(inviteRow).toBeTruthy();
  });

  it('should not show invite form for non-owner', () => {
    initAsMember();
    const inviteRow = fixture.nativeElement.querySelector('.ws-settings__invite-row');
    expect(inviteRow).toBeFalsy();
  });

  it('should display pending invitations', () => {
    initAsOwner();
    const pendingRows = fixture.nativeElement.querySelectorAll('.ws-settings__pending-row');
    expect(pendingRows.length).toBe(1);
    expect(pendingRows[0].textContent).toContain('friend@email.com');
  });

  it('should send invitation and refresh list', () => {
    initAsOwner();

    component.onInviteEmailChange('alex@email.com');
    component.sendInvite();

    const inviteReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations') && r.method === 'POST'
    );
    expect(inviteReq.request.body).toEqual({ email: 'alex@email.com' });
    inviteReq.flush({
      invitationId: 'inv-2',
      invitedEmail: 'alex@email.com',
      status: 'Pending',
      expiresAt: '2026-03-30T00:00:00Z',
      token: 'tok-2',
    });
    fixture.detectChanges();

    expect(component.inviteSuccess()).toContain('alex@email.com');

    const refreshReq = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations') && r.method === 'GET'
    );
    refreshReq.flush([...mockInvitations, {
      invitationId: 'inv-2',
      invitedEmail: 'alex@email.com',
      status: 'Pending',
      expiresAt: '2026-03-30T00:00:00Z',
      createdAt: '2026-03-25T00:00:00Z',
    }]);
    fixture.detectChanges();

    expect(component.pendingInvitations().length).toBe(2);
  });

  it('should revoke invitation after confirmation', () => {
    initAsOwner();
    vi.spyOn(globalThis, 'confirm').mockReturnValue(true);

    component.revokeInvitation('inv-1', 'friend@email.com');

    const req = httpTesting.expectOne((r) =>
      r.url.includes('/watchspaces/ws-1/invitations/inv-1') && r.method === 'DELETE'
    );
    req.flush(null);
    fixture.detectChanges();

    expect(component.pendingInvitations().length).toBe(0);
  });

  // --- Danger Zone (non-owner) ---

  it('should show danger zone for non-owner', () => {
    initAsMember();
    const dangerZone = fixture.nativeElement.querySelector('.ws-settings__danger-zone');
    expect(dangerZone).toBeTruthy();
  });

  it('should not show danger zone for owner', () => {
    initAsOwner();
    const dangerZone = fixture.nativeElement.querySelector('.ws-settings__danger-zone');
    expect(dangerZone).toBeFalsy();
  });
});
