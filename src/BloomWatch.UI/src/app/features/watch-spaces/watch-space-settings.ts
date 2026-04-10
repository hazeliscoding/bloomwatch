import {
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomInputComponent } from '../../shared/ui/input/bloom-input';
import { BloomBadgeComponent, BloomBadgeColor } from '../../shared/ui/badge/bloom-badge';
import { BloomAvatarComponent } from '../../shared/ui/avatar/bloom-avatar';
import { WatchSpaceService } from './watch-space.service';
import { InvitationDetail, WatchSpaceDetail } from './watch-space.model';
import { AuthService } from '../../core/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-watch-space-settings',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    DatePipe,
    BloomCardComponent,
    BloomButtonComponent,
    BloomInputComponent,
    BloomBadgeComponent,
    BloomAvatarComponent,
  ],
  styleUrl: './watch-space-settings.scss',
  template: `
    <main class="ws-settings">
      <!-- Loading -->
      @if (isLoading()) {
        <div class="ws-settings__loading" role="status">
          <p>Loading settings&hellip;</p>
        </div>
      }

      <!-- Error -->
      @if (!isLoading() && loadError()) {
        <div class="ws-settings__error" role="alert">
          <p>{{ loadError() }}</p>
        </div>
      }

      <!-- Settings Card -->
      @if (!isLoading() && !loadError() && detail(); as d) {
        <a class="ws-settings__back-link" [routerLink]="['/watch-spaces', d.watchSpaceId]">
          &larr; Back to Dashboard
        </a>

        <div class="ws-settings__panel">
          <bloom-card>
            <h1 bloomCardHeader class="ws-settings__title bloom-font-display">Watch Space Settings</h1>

            <div class="ws-settings__body">

              <!-- Action Error -->
              @if (actionError()) {
                <div class="ws-settings__action-error" role="alert">
                  <p>{{ actionError() }}</p>
                </div>
              }

              <!-- Space Name -->
              <section class="ws-settings__section">
                <h2 class="ws-settings__section-title">Space Name</h2>
                @if (isEditing()) {
                  <div class="ws-settings__rename-form">
                    <bloom-input
                      [ngModel]="editName()"
                      (valueChange)="onEditNameChange($event)"
                      placeholder="Space name"
                    />
                    @if (saveError()) {
                      <p class="ws-settings__save-error" role="alert">{{ saveError() }}</p>
                    }
                    <div class="ws-settings__rename-actions">
                      <bloom-button
                        variant="primary"
                        size="sm"
                        [disabled]="isSaving()"
                        [loading]="isSaving()"
                        (clicked)="saveRename()"
                      >
                        Save
                      </bloom-button>
                      <bloom-button
                        variant="ghost"
                        size="sm"
                        [disabled]="isSaving()"
                        (clicked)="cancelEditing()"
                      >
                        Cancel
                      </bloom-button>
                    </div>
                  </div>
                } @else {
                  <div class="ws-settings__inline-edit">
                    <span>{{ d.name }}</span>
                    @if (isOwner()) {
                      <button
                        class="ws-settings__edit-icon"
                        (click)="startEditing()"
                        title="Rename"
                        aria-label="Rename space"
                      >&#9998;</button>
                    }
                  </div>
                  @if (isOwner()) {
                    <span class="ws-settings__hint">Only the owner can rename this space</span>
                  }
                }
              </section>

              <!-- Members -->
              <section class="ws-settings__section">
                <h2 class="ws-settings__section-title">Members ({{ d.members.length }})</h2>
                <div class="ws-settings__member-list">
                  @for (member of d.members; track member.userId) {
                    <div class="ws-settings__member-row">
                      <bloom-avatar size="sm" [name]="member.displayName" />
                      <div class="ws-settings__member-info">
                        <div class="ws-settings__member-name">{{ member.displayName }}</div>
                        <div class="ws-settings__member-date">Joined {{ member.joinedAt | date:'mediumDate' }}</div>
                      </div>
                      <bloom-badge [color]="roleBadgeColor(member.role)" size="sm">{{ member.role }}</bloom-badge>
                      @if (isOwner() && !isSelf(member.userId)) {
                        <div class="ws-settings__member-actions">
                          <bloom-button variant="ghost" size="sm" (clicked)="transferOwnership(member.userId, member.displayName)">
                            Transfer
                          </bloom-button>
                          <bloom-button variant="danger" size="sm" (clicked)="removeMember(member.userId, member.displayName)">
                            Remove
                          </bloom-button>
                        </div>
                      }
                    </div>
                  }
                </div>
              </section>

              <!-- Invite a Member (owner only) -->
              @if (isOwner()) {
                <section class="ws-settings__section">
                  <h2 class="ws-settings__section-title">Invite a Member</h2>
                  <div class="ws-settings__invite-row">
                    <bloom-input
                      placeholder="friend&#64;example.com"
                      [ngModel]="inviteEmail()"
                      (valueChange)="onInviteEmailChange($event)"
                    />
                    <bloom-button
                      variant="primary"
                      size="md"
                      [disabled]="isInviting() || !inviteEmail().trim()"
                      [loading]="isInviting()"
                      (clicked)="sendInvite()"
                    >
                      Send
                    </bloom-button>
                  </div>
                  @if (inviteError()) {
                    <p class="ws-settings__invite-error" role="alert">{{ inviteError() }}</p>
                  }
                  @if (inviteSuccess()) {
                    <p class="ws-settings__invite-success">{{ inviteSuccess() }}</p>
                  }
                </section>

                <!-- Pending Invitations -->
                <section class="ws-settings__section">
                  <h2 class="ws-settings__section-title">Pending Invitations</h2>
                  @if (isLoadingInvitations()) {
                    <p class="ws-settings__hint">Loading invitations&hellip;</p>
                  } @else if (pendingInvitations().length === 0) {
                    <p class="ws-settings__hint">No pending invitations</p>
                  } @else {
                    <div class="ws-settings__pending-list">
                      @for (inv of pendingInvitations(); track inv.invitationId) {
                        <div class="ws-settings__pending-row">
                          <div>
                            <div class="ws-settings__pending-email">{{ inv.invitedEmail }}</div>
                            <div class="ws-settings__pending-dates">
                              Sent: {{ inv.createdAt | date:'mediumDate' }} &middot; Expires: {{ inv.expiresAt | date:'mediumDate' }}
                            </div>
                          </div>
                          <bloom-button variant="danger" size="sm" (clicked)="revokeInvitation(inv.invitationId, inv.invitedEmail)">
                            Revoke
                          </bloom-button>
                        </div>
                      }
                    </div>
                  }
                </section>
              }

              <!-- Danger Zone (non-owner) -->
              @if (!isOwner()) {
                <section class="ws-settings__section">
                  <h2 class="ws-settings__section-title">Danger Zone</h2>
                  <div class="ws-settings__danger-zone">
                    <p>Leaving this space will remove your membership. You&rsquo;ll need a new invitation to rejoin.</p>
                    <bloom-button variant="danger" size="md" (clicked)="leaveSpace()">
                      Leave Watch Space
                    </bloom-button>
                  </div>
                </section>
              }

            </div>
          </bloom-card>
        </div>
      }
    </main>
  `,
})
export class WatchSpaceSettings implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly watchSpaceService = inject(WatchSpaceService);
  private readonly authService = inject(AuthService);
  private readonly titleService = inject(Title);

  readonly detail = signal<WatchSpaceDetail | null>(null);
  readonly isLoading = signal(true);
  readonly loadError = signal('');

  readonly currentUserId = computed(() => this.authService.userId());

  readonly isOwner = computed(() => {
    const d = this.detail();
    const uid = this.currentUserId();
    if (!d || !uid) return false;
    return d.members.some(m => m.userId === uid && m.role === 'Owner');
  });

  // Inline rename
  readonly isEditing = signal(false);
  readonly editName = signal('');
  readonly isSaving = signal(false);
  readonly saveError = signal('');

  // Action feedback
  readonly actionError = signal('');

  // Invite form
  readonly inviteEmail = signal('');
  readonly isInviting = signal(false);
  readonly inviteError = signal('');
  readonly inviteSuccess = signal('');

  // Invitations
  readonly invitations = signal<InvitationDetail[]>([]);
  readonly isLoadingInvitations = signal(false);

  readonly pendingInvitations = computed(() =>
    this.invitations().filter(i => i.status === 'Pending')
  );

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDetail(id);
    }
  }

  // --- Rename ---

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

  // --- Members ---

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
        this.actionError.set('Failed to transfer ownership. Please try again.');
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

  // --- Invitations ---

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

  // --- Helpers ---

  roleBadgeColor(role: string): BloomBadgeColor {
    return role === 'Owner' ? 'pink' : 'blue';
  }

  isSelf(userId: string): boolean {
    return userId === this.currentUserId();
  }

  // --- Data loading ---

  private loadDetail(id: string): void {
    this.isLoading.set(true);
    this.loadError.set('');

    this.watchSpaceService.getWatchSpaceById(id).subscribe({
      next: (detail) => {
        this.detail.set(detail);
        this.isLoading.set(false);
        this.titleService.setTitle(`${detail.name} · Settings · BloomWatch`);
        this.loadInvitations();
      },
      error: () => {
        this.isLoading.set(false);
        this.loadError.set('Could not load watch space details. Please try again later.');
      },
    });
  }

  private loadInvitations(): void {
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
}
