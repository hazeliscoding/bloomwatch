import { Component, computed, inject, OnInit, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomInputComponent } from '../../shared/ui/input/bloom-input';
import { WatchSpaceService } from './watch-space.service';
import { InvitationDetail, WatchSpaceDetail as WatchSpaceDetailModel } from './watch-space.model';
import { AuthService } from '../../core/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { AnimeSearchModalComponent } from './anime-search-modal';
import { AnimeListComponent } from './anime-list';

@Component({
  selector: 'app-watch-space-detail',
  imports: [FormsModule, RouterLink, BloomButtonComponent, BloomInputComponent, AnimeSearchModalComponent, AnimeListComponent],
  templateUrl: './watch-space-detail.html',
  styleUrl: './watch-space-detail.scss',
})
export class WatchSpaceDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly watchSpaceService = inject(WatchSpaceService);
  private readonly authService = inject(AuthService);
  private readonly titleService = inject(Title);

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

  // Invite form state
  readonly inviteEmail = signal('');
  readonly isInviting = signal(false);
  readonly inviteError = signal('');
  readonly inviteSuccess = signal('');

  // Invitations list state
  readonly invitations = signal<InvitationDetail[]>([]);
  readonly isLoadingInvitations = signal(false);

  // Anime list ref
  private readonly animeList = viewChild<AnimeListComponent>('animeList');

  // Anime search modal state
  readonly isSearchModalOpen = signal(false);

  openSearchModal(): void {
    this.isSearchModalOpen.set(true);
  }

  private animeAddedDuringSearch = false;

  onAnimeAdded(): void {
    this.animeAddedDuringSearch = true;
  }

  onSearchModalClosedWithRefresh(): void {
    this.isSearchModalOpen.set(false);
    if (this.animeAddedDuringSearch) {
      this.animeList()?.refresh();
      this.animeAddedDuringSearch = false;
    }
  }

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

  onInviteEmailChange(value: string): void {
    this.inviteEmail.set(value);
    this.inviteError.set('');
    this.inviteSuccess.set('');
  }

  sendInvite(): void {
    const d = this.detail();
    const email = this.inviteEmail().trim();
    if (!d || !email) return;

    this.isInviting.set(true);
    this.inviteError.set('');
    this.inviteSuccess.set('');

    this.watchSpaceService.sendInvitation(d.watchSpaceId, email).subscribe({
      next: () => {
        this.inviteSuccess.set(`Invitation sent to ${email}`);
        this.inviteEmail.set('');
        this.isInviting.set(false);
        this.loadInvitations();
      },
      error: (err: HttpErrorResponse) => {
        this.isInviting.set(false);
        if (err.status === 409) {
          this.inviteError.set('This user is already a member or has a pending invitation.');
        } else if (err.status === 422) {
          this.inviteError.set('This email is not a registered BloomWatch user.');
        } else {
          this.inviteError.set('Failed to send invitation. Please try again.');
        }
      },
    });
  }

  revokeInvitation(invitationId: string, email: string): void {
    const d = this.detail();
    if (!d) return;
    if (!confirm(`Revoke invitation to ${email}?`)) return;

    this.actionError.set('');
    this.watchSpaceService.revokeInvitation(d.watchSpaceId, invitationId).subscribe({
      next: () => {
        this.invitations.update(prev => prev.filter(inv => inv.invitationId !== invitationId));
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 409) {
          this.actionError.set('This invitation has already been accepted or declined.');
        } else {
          this.actionError.set(`Failed to revoke invitation to ${email}. Please try again.`);
        }
      },
    });
  }

  loadInvitations(): void {
    const d = this.detail();
    if (!d || !this.isOwner()) return;

    this.isLoadingInvitations.set(true);
    this.watchSpaceService.listInvitations(d.watchSpaceId).subscribe({
      next: (invitations) => {
        this.invitations.set(invitations);
        this.isLoadingInvitations.set(false);
      },
      error: () => {
        this.isLoadingInvitations.set(false);
      },
    });
  }

  roleBadgeColor(role: string): string {
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
        this.titleService.setTitle(`${detail.name} · Anime List · BloomWatch`);
        this.loadInvitations();
      },
      error: () => {
        this.isLoading.set(false);
        this.loadError.set('Could not load watch space details. Please try again later.');
      },
    });
  }
}
