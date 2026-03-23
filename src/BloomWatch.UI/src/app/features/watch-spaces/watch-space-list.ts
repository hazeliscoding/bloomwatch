import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomBadgeComponent } from '../../shared/ui/badge/bloom-badge';
import { BloomInputComponent } from '../../shared/ui/input/bloom-input';
import { BloomModalComponent } from '../../shared/ui/modal/bloom-modal';
import { BloomAvatarComponent, BloomAvatarStackComponent } from '../../shared/ui/avatar/bloom-avatar';
import { WatchSpaceService } from './watch-space.service';
import { WatchSpaceSummary } from './watch-space.model';
import { BloomBadgeColor } from '../../shared/ui/badge/bloom-badge';

@Component({
  selector: 'app-watch-space-list',
  imports: [
    BloomCardComponent,
    BloomButtonComponent,
    BloomBadgeComponent,
    BloomInputComponent,
    BloomModalComponent,
    BloomAvatarComponent,
    BloomAvatarStackComponent,
  ],
  templateUrl: './watch-space-list.html',
  styleUrl: './watch-space-list.scss',
})
export class WatchSpaceList implements OnInit {
  private readonly watchSpaceService = inject(WatchSpaceService);
  private readonly router = inject(Router);

  readonly spaces = signal<WatchSpaceSummary[]>([]);
  readonly isLoading = signal(true);
  readonly loadError = signal('');

  readonly showCreateModal = signal(false);
  readonly newSpaceName = signal('');
  readonly createError = signal('');
  readonly nameValidationError = signal('');
  readonly isCreating = signal(false);

  ngOnInit(): void {
    this.loadSpaces();
  }

  openCreateModal(): void {
    this.showCreateModal.set(true);
    this.createError.set('');
    this.nameValidationError.set('');
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
    this.newSpaceName.set('');
    this.createError.set('');
    this.nameValidationError.set('');
  }

  onNameChange(value: string): void {
    this.newSpaceName.set(value);
    this.nameValidationError.set('');
    this.createError.set('');
  }

  submitCreate(event: Event): void {
    event.preventDefault();

    const name = this.newSpaceName().trim();
    if (!name) {
      this.nameValidationError.set('Name is required');
      return;
    }

    this.isCreating.set(true);
    this.createError.set('');

    this.watchSpaceService.createWatchSpace(name).subscribe({
      next: (created) => {
        this.spaces.update((list) => [...list, created]);
        this.isCreating.set(false);
        this.closeCreateModal();
      },
      error: () => {
        this.isCreating.set(false);
        this.createError.set('Failed to create watch space. Please try again.');
      },
    });
  }

  navigateToSpace(spaceId: string): void {
    this.router.navigate(['/watch-spaces', spaceId]);
  }

  roleBadgeColor(role: string): BloomBadgeColor {
    return role === 'Owner' ? 'pink' : 'blue';
  }

  private loadSpaces(): void {
    this.isLoading.set(true);
    this.loadError.set('');

    this.watchSpaceService.getMyWatchSpaces().subscribe({
      next: (spaces) => {
        this.spaces.set(spaces);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.loadError.set('Could not load your watch spaces. Please try again later.');
      },
    });
  }
}
