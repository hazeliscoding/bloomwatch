import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomBadgeComponent, BloomBadgeColor } from '../../shared/ui/badge/bloom-badge';
import { BloomInputComponent } from '../../shared/ui/input/bloom-input';
import { WatchSpaceService } from './watch-space.service';
import { WatchSpaceDetail as WatchSpaceDetailModel } from './watch-space.model';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-watch-space-detail',
  imports: [DatePipe, FormsModule, BloomCardComponent, BloomButtonComponent, BloomBadgeComponent, BloomInputComponent],
  templateUrl: './watch-space-detail.html',
  styleUrl: './watch-space-detail.scss',
})
export class WatchSpaceDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly watchSpaceService = inject(WatchSpaceService);
  private readonly authService = inject(AuthService);

  readonly detail = signal<WatchSpaceDetailModel | null>(null);
  readonly isLoading = signal(true);
  readonly loadError = signal('');

  readonly currentUserId = computed(() => this.authService.userId());

  readonly isOwner = computed(() => {
    const d = this.detail();
    const uid = this.currentUserId();
    if (!d || !uid) return false;
    return d.members.some(m => m.userId === uid && m.role === 'Owner');
  });

  // Inline rename state
  readonly isEditing = signal(false);
  readonly editName = signal('');
  readonly isSaving = signal(false);
  readonly saveError = signal('');

  // Action feedback
  readonly actionError = signal('');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDetail(id);
    }
  }

  startEditing(): void {
    const d = this.detail();
    if (!d) return;
    this.editName.set(d.name);
    this.isEditing.set(true);
    this.saveError.set('');
  }

  cancelEditing(): void {
    this.isEditing.set(false);
    this.editName.set('');
    this.saveError.set('');
  }

  onEditNameChange(value: string): void {
    this.editName.set(value);
    this.saveError.set('');
  }

  saveRename(): void {
    const d = this.detail();
    const name = this.editName().trim();
    if (!d || !name) return;

    this.isSaving.set(true);
    this.saveError.set('');

    this.watchSpaceService.renameWatchSpace(d.watchSpaceId, name).subscribe({
      next: () => {
        this.detail.update(prev => prev ? { ...prev, name } : prev);
        this.isEditing.set(false);
        this.isSaving.set(false);
      },
      error: () => {
        this.isSaving.set(false);
        this.saveError.set('Failed to rename. Please try again.');
      },
    });
  }

  removeMember(userId: string, displayName: string): void {
    const d = this.detail();
    if (!d) return;
    if (!confirm(`Remove ${displayName} from this watch space?`)) return;

    this.actionError.set('');
    this.watchSpaceService.removeMember(d.watchSpaceId, userId).subscribe({
      next: () => {
        this.detail.update(prev =>
          prev ? { ...prev, members: prev.members.filter(m => m.userId !== userId) } : prev
        );
      },
      error: () => {
        this.actionError.set(`Failed to remove ${displayName}. Please try again.`);
      },
    });
  }

  transferOwnership(newOwnerId: string, displayName: string): void {
    const d = this.detail();
    if (!d) return;
    if (!confirm(`Transfer ownership to ${displayName}? You will become a regular member.`)) return;

    this.actionError.set('');
    this.watchSpaceService.transferOwnership(d.watchSpaceId, newOwnerId).subscribe({
      next: () => {
        this.detail.update(prev => {
          if (!prev) return prev;
          return {
            ...prev,
            members: prev.members.map(m => {
              if (m.userId === newOwnerId) return { ...m, role: 'Owner' };
              if (m.role === 'Owner') return { ...m, role: 'Member' };
              return m;
            }),
          };
        });
      },
      error: () => {
        this.actionError.set(`Failed to transfer ownership. Please try again.`);
      },
    });
  }

  leaveSpace(): void {
    const d = this.detail();
    if (!d) return;
    if (!confirm('Leave this watch space? You will lose access.')) return;

    this.actionError.set('');
    this.watchSpaceService.leaveWatchSpace(d.watchSpaceId).subscribe({
      next: () => {
        this.router.navigate(['/watch-spaces']);
      },
      error: () => {
        this.actionError.set('Failed to leave. You may be the sole owner.');
      },
    });
  }

  roleBadgeColor(role: string): BloomBadgeColor {
    return role === 'Owner' ? 'pink' : 'blue';
  }

  isSelf(userId: string): boolean {
    return userId === this.currentUserId();
  }

  private loadDetail(id: string): void {
    this.isLoading.set(true);
    this.loadError.set('');

    this.watchSpaceService.getWatchSpaceById(id).subscribe({
      next: (detail) => {
        this.detail.set(detail);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.loadError.set('Could not load watch space details. Please try again later.');
      },
    });
  }
}
