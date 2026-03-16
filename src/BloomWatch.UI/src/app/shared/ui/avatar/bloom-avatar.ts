import { Component, input, computed } from '@angular/core';
import { NgClass } from '@angular/common';

export type BloomAvatarSize = 'xs' | 'sm' | 'md' | 'lg';
export type BloomAvatarStatus = 'online' | 'offline' | 'watching' | 'none';

@Component({
  selector: 'bloom-avatar',
  standalone: true,
  imports: [NgClass],
  styleUrl: './bloom-avatar.scss',
  template: `
    <div [ngClass]="avatarClasses()" [attr.aria-label]="ariaLabel()">
      <div class="bloom-avatar__frame">
        @if (src()) {
          <img
            class="bloom-avatar__image"
            [src]="src()"
            [alt]="alt() || name() || 'User avatar'"
            loading="lazy"
          />
        } @else {
          <span class="bloom-avatar__initials" aria-hidden="true">
            {{ initials() }}
          </span>
        }
      </div>

      @if (status() !== 'none') {
        <span
          class="bloom-avatar__status"
          [ngClass]="'bloom-avatar__status--' + status()"
          [attr.aria-label]="status()"
          role="status"
        ></span>
      }
    </div>
  `,
})
export class BloomAvatarComponent {
  readonly src = input<string>('');
  readonly alt = input<string>('');
  readonly name = input<string>('');
  readonly size = input<BloomAvatarSize>('md');
  readonly status = input<BloomAvatarStatus>('none');
  readonly ariaLabel = input<string | undefined>(undefined);

  readonly initials = computed(() => {
    const n = this.name();
    if (!n) return '?';
    const parts = n.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  });

  readonly avatarClasses = computed(() => {
    return {
      'bloom-avatar': true,
      [`bloom-avatar--${this.size()}`]: true,
    };
  });
}

// --------------------------------------------------------------------------
// Avatar Stack Component (shows overlapping group of avatars)
// --------------------------------------------------------------------------

@Component({
  selector: 'bloom-avatar-stack',
  standalone: true,
  styleUrl: './bloom-avatar.scss',
  template: `
    <div class="bloom-avatar-stack" [attr.aria-label]="ariaLabel()">
      <ng-content />
    </div>
  `,
})
export class BloomAvatarStackComponent {
  readonly ariaLabel = input<string>('User group');
}
