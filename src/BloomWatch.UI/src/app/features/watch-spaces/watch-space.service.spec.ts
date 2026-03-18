import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { WatchSpaceService } from './watch-space.service';
import { WatchSpaceSummary } from './watch-space.model';

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
});
