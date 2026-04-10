import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { WatchSpaceService } from './watch-space.service';
import { InvitationPreview } from './watch-space.model';
import { HttpErrorResponse } from '@angular/common/http';

type PageState = 'loading' | 'ready' | 'accepting' | 'declining' | 'accepted' | 'declined' | 'expired' | 'already-used' | 'error';

@Component({
  selector: 'app-invitation-response',
  imports: [BloomButtonComponent, BloomCardComponent, RouterLink],
  templateUrl: './invitation-response.html',
  styleUrl: './invitation-response.scss',
})
export class InvitationResponse implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly watchSpaceService = inject(WatchSpaceService);

  readonly state = signal<PageState>('loading');
  readonly preview = signal<InvitationPreview | null>(null);
  readonly errorMessage = signal('');
  readonly acceptedWatchSpaceId = signal('');

  private token = '';

  private autoAction: 'accept' | 'decline' | null = null;

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token') ?? '';
    if (!this.token) {
      this.state.set('error');
      this.errorMessage.set('No invitation token provided.');
      return;
    }
    this.autoAction = this.route.snapshot.data?.['action'] ?? null;
    this.loadPreview();
  }

  accept(): void {
    this.state.set('accepting');
    this.watchSpaceService.acceptInvitation(this.token).subscribe({
      next: (result) => {
        this.acceptedWatchSpaceId.set(result.watchSpaceId);
        this.state.set('accepted');
      },
      error: (err: HttpErrorResponse) => {
        this.setErrorState(err.status);
      },
    });
  }

  decline(): void {
    this.state.set('declining');
    this.watchSpaceService.declineInvitation(this.token).subscribe({
      next: () => {
        this.state.set('declined');
      },
      error: (err: HttpErrorResponse) => {
        this.setErrorState(err.status);
      },
    });
  }

  goToWatchSpace(): void {
    const id = this.acceptedWatchSpaceId();
    if (id) {
      this.router.navigate(['/watch-spaces', id]);
    }
  }

  goToWatchSpaces(): void {
    this.router.navigate(['/watch-spaces']);
  }

  private loadPreview(): void {
    this.watchSpaceService.getInvitationPreview(this.token).subscribe({
      next: (preview) => {
        if (preview.status !== 'Pending') {
          this.state.set('already-used');
          return;
        }
        this.preview.set(preview);
        if (this.autoAction === 'accept') {
          this.accept();
        } else if (this.autoAction === 'decline') {
          this.decline();
        } else {
          this.state.set('ready');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.setErrorState(err.status);
      },
    });
  }

  private setErrorState(status: number): void {
    switch (status) {
      case 409:
        this.state.set('already-used');
        break;
      case 410:
        this.state.set('expired');
        break;
      default:
        this.state.set('error');
        this.errorMessage.set(this.mapError(status));
        break;
    }
  }

  private mapError(status: number): string {
    switch (status) {
      case 403:
        return 'This invitation was sent to a different account.';
      case 404:
        return 'This invitation was not found. It may have been revoked.';
      default:
        return 'Something went wrong. Please try again.';
    }
  }
}
