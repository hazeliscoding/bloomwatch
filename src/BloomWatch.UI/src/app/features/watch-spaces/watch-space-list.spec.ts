import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { WatchSpaceList } from './watch-space-list';
import { WatchSpaceSummary } from './watch-space.model';

describe('WatchSpaceList', () => {
  let component: WatchSpaceList;
  let fixture: ComponentFixture<WatchSpaceList>;
  let httpTesting: HttpTestingController;
  let router: Router;

  const mockSpaces: WatchSpaceSummary[] = [
    { watchSpaceId: 'ws-1', name: 'Anime Club', createdAt: '2026-01-15T00:00:00Z', role: 'Owner' },
    { watchSpaceId: 'ws-2', name: 'Watch Party', createdAt: '2026-02-20T00:00:00Z', role: 'Member' },
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [WatchSpaceList],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    fixture = TestBed.createComponent(WatchSpaceList);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function flushSpaces(spaces: WatchSpaceSummary[] = mockSpaces): void {
    const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces'));
    req.flush(spaces);
    fixture.detectChanges();
  }

  function flushSpacesError(): void {
    const req = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces'));
    req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
    fixture.detectChanges();
  }

  // ---- Loading State ----

  it('should show loading state initially', () => {
    fixture.detectChanges();
    expect(component.isLoading()).toBe(true);
    httpTesting.expectOne((r) => r.url.endsWith('/watchspaces')).flush(mockSpaces);
  });

  it('should clear loading state after spaces load', () => {
    fixture.detectChanges();
    flushSpaces();
    expect(component.isLoading()).toBe(false);
  });

  // ---- Spaces Rendering ----

  it('should display space cards after loading', () => {
    fixture.detectChanges();
    flushSpaces();
    expect(component.spaces().length).toBe(2);
    expect(component.spaces()[0].name).toBe('Anime Club');
    expect(component.spaces()[1].name).toBe('Watch Party');
  });

  // ---- Empty State ----

  it('should show empty state when no spaces exist', () => {
    fixture.detectChanges();
    flushSpaces([]);
    expect(component.spaces().length).toBe(0);
    expect(component.isLoading()).toBe(false);
    expect(component.loadError()).toBe('');
  });

  // ---- Error State ----

  it('should show error state when API fails', () => {
    fixture.detectChanges();
    flushSpacesError();
    expect(component.loadError()).toBeTruthy();
    expect(component.isLoading()).toBe(false);
  });

  // ---- Navigation ----

  it('should navigate to space detail on navigateToSpace', () => {
    vi.spyOn(router, 'navigate');
    fixture.detectChanges();
    flushSpaces();

    component.navigateToSpace('ws-1');
    expect(router.navigate).toHaveBeenCalledWith(['/watch-spaces', 'ws-1']);
  });

  // ---- Create Form Toggle ----

  it('should toggle create form visibility', () => {
    fixture.detectChanges();
    flushSpaces();

    expect(component.showCreateForm()).toBe(false);
    component.openCreateForm();
    expect(component.showCreateForm()).toBe(true);
    component.cancelCreate();
    expect(component.showCreateForm()).toBe(false);
  });

  it('should clear input on cancel', () => {
    fixture.detectChanges();
    flushSpaces();

    component.openCreateForm();
    component.onNameChange('Test');
    expect(component.newSpaceName()).toBe('Test');

    component.cancelCreate();
    expect(component.newSpaceName()).toBe('');
  });

  // ---- Blank Name Validation ----

  it('should show validation error for blank name', () => {
    fixture.detectChanges();
    flushSpaces();

    component.openCreateForm();
    component.onNameChange('   ');
    component.submitCreate(new Event('submit'));

    expect(component.nameValidationError()).toBe('Name is required');
    expect(component.isCreating()).toBe(false);
  });

  // ---- Successful Creation ----

  it('should create space and add to list', () => {
    fixture.detectChanges();
    flushSpaces();

    component.openCreateForm();
    component.onNameChange('New Space');
    component.submitCreate(new Event('submit'));

    expect(component.isCreating()).toBe(true);

    const createReq = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces'));
    expect(createReq.request.method).toBe('POST');
    expect(createReq.request.body).toEqual({ name: 'New Space' });

    const created: WatchSpaceSummary = {
      watchSpaceId: 'ws-3',
      name: 'New Space',
      createdAt: '2026-03-18T00:00:00Z',
      role: 'Owner',
    };
    createReq.flush(created);
    fixture.detectChanges();

    expect(component.spaces().length).toBe(3);
    expect(component.spaces()[2].name).toBe('New Space');
    expect(component.showCreateForm()).toBe(false);
    expect(component.isCreating()).toBe(false);
  });

  // ---- API Error During Creation ----

  it('should show create error when API fails', () => {
    fixture.detectChanges();
    flushSpaces();

    component.openCreateForm();
    component.onNameChange('Fail Space');
    component.submitCreate(new Event('submit'));

    const createReq = httpTesting.expectOne((r) => r.url.endsWith('/watchspaces'));
    createReq.flush('Error', { status: 500, statusText: 'Internal Server Error' });
    fixture.detectChanges();

    expect(component.createError()).toBeTruthy();
    expect(component.showCreateForm()).toBe(true);
    expect(component.newSpaceName()).toBe('Fail Space');
    expect(component.isCreating()).toBe(false);
  });

  // ---- Role Badge Color ----

  it('should return pink for Owner and blue for Member', () => {
    expect(component.roleBadgeColor('Owner')).toBe('pink');
    expect(component.roleBadgeColor('Member')).toBe('blue');
  });
});
