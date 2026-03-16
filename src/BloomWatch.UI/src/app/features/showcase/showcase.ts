import { Component } from '@angular/core';
import {
  BloomButtonComponent,
  BloomCardComponent,
  BloomInputComponent,
  BloomBadgeComponent,
  BloomAvatarComponent,
  BloomAvatarStackComponent,
} from '../../shared/ui';

@Component({
  selector: 'app-showcase',
  standalone: true,
  imports: [
    BloomButtonComponent,
    BloomCardComponent,
    BloomInputComponent,
    BloomBadgeComponent,
    BloomAvatarComponent,
    BloomAvatarStackComponent,
  ],
  styleUrl: './showcase.scss',
  templateUrl: './showcase.html',
})
export class Showcase {}
