import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';
import { WatchSpaceService } from './watch-space.service';
import { InvitationPreview } from './watch-space.model';
import { HttpErrorResponse } from '@angular/common/http';

type PageState = 'loading' | 'ready' | 'accepting' | 'declining' | 'accepted' | 'declined' | 'error';

@Component({
  selector: 'app-invitation-response',
  imports: [BloomButtonComponent, BloomCardComponent],
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

  private token = '';

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token') ?? '';
    if (!this.token) {
      this.state.set('error');
      this.errorMessage.set('No invitation token provided.');
      return;
    }
    this.loadPreview();
  }

  accept(): void {
    this.state.set('accepting');
    this.watchSpaceService.acceptInvitation(this.token).subscribe({
      next: (result) => {
        this.state.set('accepted');
        this.router.navigate(['/watch-spaces', result.watchSpaceId]);
      },
      error: (err: HttpErrorResponse) => {
        this.state.set('error');
        this.errorMessage.set(this.mapError(err.status));
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
        this.state.set('error');
        this.errorMessage.set(this.mapError(err.status));
      },
    });
  }

  private loadPreview(): void {
    this.watchSpaceService.getInvitationPreview(this.token).subscribe({
      next: (preview) => {
        if (preview.status !== 'Pending') {
          this.state.set('error');
          this.errorMessage.set('This invitation has already been used.');
          return;
        }
        this.preview.set(preview);
        this.state.set('ready');
      },
      error: (err: HttpErrorResponse) => {
        this.state.set('error');
        this.errorMessage.set(this.mapError(err.status));
      },
    });
  }

  goToWatchSpaces(): void {
    this.router.navigate(['/watch-spaces']);
  }

  private mapError(status: number): string {
    switch (status) {
      case 403:
        return 'This invitation was sent to a different account.';
      case 404:
        return 'This invitation was not found. It may have been revoked.';
      case 409:
        return 'This invitation has already been used.';
      case 410:
        return 'This invitation has expired.';
      default:
        return 'Something went wrong. Please try again.';
    }
  }
}
