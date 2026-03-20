import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { WatchSpaceService } from './watch-space.service';
import { InvitationDetail, InviteMemberResponse, InvitationPreview, AcceptInvitationResponse, WatchSpaceSummary } from './watch-space.model';

describe('WatchSpaceService', () => {
  let service: WatchSpaceService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(WatchSpaceService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  describe('getMyWatchSpaces', () => {
    it('should send GET /watchspaces and return spaces', () => {
      const mockSpaces: WatchSpaceSummary[] = [
        { watchSpaceId: '1', name: 'Space One', createdAt: '2026-01-01T00:00:00Z', role: 'Owner' },
        { watchSpaceId: '2', name: 'Space Two', createdAt: '2026-02-01T00:00:00Z', role: 'Member' },
      ];

      service.getMyWatchSpaces().subscribe((spaces) => {
        expect(spaces).toEqual(mockSpaces);
      });

      const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces'));
      expect(req.request.method).toBe('GET');
      req.flush(mockSpaces);
    });
  });

  describe('createWatchSpace', () => {
    it('should send POST /watchspaces with name and return created space', () => {
      const created: WatchSpaceSummary = {
        watchSpaceId: '3',
        name: 'New Space',
        createdAt: '2026-03-01T00:00:00Z',
        role: 'Owner',
      };

      service.createWatchSpace('New Space').subscribe((space) => {
        expect(space).toEqual(created);
      });

      const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces'));
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ name: 'New Space' });
      req.flush(created);
    });
  });

  describe('sendInvitation', () => {
    it('should send POST /watchspaces/{id}/invitations with email', () => {
      const result: InviteMemberResponse = {
        invitationId: 'inv-1',
        invitedEmail: 'friend@example.com',
        status: 'Pending',
        expiresAt: '2026-03-26T00:00:00Z',
        token: 'abc123',
      };

      service.sendInvitation('space-1', 'friend@example.com').subscribe((res) => {
        expect(res).toEqual(result);
      });

      const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/space-1/invitations'));
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ email: 'friend@example.com' });
      req.flush(result);
    });
  });

  describe('listInvitations', () => {
    it('should send GET /watchspaces/{id}/invitations', () => {
      const invitations: InvitationDetail[] = [
        {
          invitationId: 'inv-1',
          invitedEmail: 'friend@example.com',
          status: 'Pending',
          expiresAt: '2026-03-26T00:00:00Z',
          createdAt: '2026-03-19T00:00:00Z',
        },
      ];

      service.listInvitations('space-1').subscribe((res) => {
        expect(res).toEqual(invitations);
      });

      const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/space-1/invitations'));
      expect(req.request.method).toBe('GET');
      req.flush(invitations);
    });
  });

  describe('revokeInvitation', () => {
    it('should send DELETE /watchspaces/{id}/invitations/{invitationId}', () => {
      service.revokeInvitation('space-1', 'inv-1').subscribe();

      const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/space-1/invitations/inv-1'));
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('getInvitationPreview', () => {
    it('should send GET /watchspaces/invitations/{token}', () => {
      const preview: InvitationPreview = {
        watchSpaceId: 'space-1',
        watchSpaceName: 'Anime Club',
        invitedEmail: 'friend@example.com',
        status: 'Pending',
        expiresAt: '2026-03-26T00:00:00Z',
      };

      service.getInvitationPreview('abc123').subscribe((res) => {
        expect(res).toEqual(preview);
      });

      const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/invitations/abc123'));
      expect(req.request.method).toBe('GET');
      req.flush(preview);
    });
  });

  describe('acceptInvitation', () => {
    it('should send POST /watchspaces/invitations/{token}/accept', () => {
      const result: AcceptInvitationResponse = { watchSpaceId: 'space-1' };

      service.acceptInvitation('abc123').subscribe((res) => {
        expect(res).toEqual(result);
      });

      const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/invitations/abc123/accept'));
      expect(req.request.method).toBe('POST');
      req.flush(result);
    });
  });

  describe('declineInvitation', () => {
    it('should send POST /watchspaces/invitations/{token}/decline', () => {
      service.declineInvitation('abc123').subscribe();

      const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/invitations/abc123/decline'));
      expect(req.request.method).toBe('POST');
      req.flush(null);
    });
  });

  describe('ensureMediaCached', () => {
    it('should send GET /api/anilist/media/{id}', () => {
      service.ensureMediaCached(12345).subscribe();

      const req = httpTesting.expectOne((r) => r.url.endsWith('/api/anilist/media/12345'));
      expect(req.request.method).toBe('GET');
      req.flush({});
    });
  });

  describe('listWatchSpaceAnime', () => {
    it('should send GET /watchspaces/{id}/anime and unwrap items', () => {
      const items = [
        { watchSpaceAnimeId: 'a1', anilistMediaId: 1, preferredTitle: 'Cowboy Bebop', coverImageUrlSnapshot: null, episodeCountSnapshot: 26, sharedStatus: 'Backlog', sharedEpisodesWatched: 0, addedAtUtc: '2026-01-01T00:00:00Z' },
      ];

      service.listWatchSpaceAnime('space-1').subscribe((res) => {
        expect(res).toEqual(items);
      });

      const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces/space-1/anime'));
      expect(req.request.method).toBe('GET');
      req.flush({ items });
    });
  });
});
