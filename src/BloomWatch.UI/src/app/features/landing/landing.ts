import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BloomButtonComponent } from '../../shared/ui/button/bloom-button';
import { BloomCardComponent } from '../../shared/ui/card/bloom-card';

@Component({
  selector: 'app-landing',
  imports: [RouterLink, BloomButtonComponent, BloomCardComponent],
  styleUrl: './landing.scss',
  template: `
    <main class="landing">
      <!-- Hero -->
      <section class="landing__hero">
        <h1 class="landing__title bloom-gradient-text bloom-font-display">BloomWatch</h1>
        <p class="landing__subtitle">
          Track anime together. Rate, compare, and bloom with your watch partner.
        </p>
        <div class="landing__ctas">
          <a routerLink="/register">
            <bloom-button variant="primary" size="lg">Create Account</bloom-button>
          </a>
          <a routerLink="/login">
            <bloom-button variant="ghost" size="lg">Sign In</bloom-button>
          </a>
        </div>
      </section>

      <!-- Kawaii Divider -->
      <div class="bloom-divider-kawaii landing__divider" aria-hidden="true"></div>

      <!-- Feature Highlights -->
      <section class="landing__features" aria-label="Features">
        <bloom-card [hoverable]="false" ariaLabel="Shared Backlog">
          <span class="landing__feature-icon" aria-hidden="true">&#9825;</span>
          <h3 class="landing__feature-title">Shared Backlog</h3>
          <p class="landing__feature-desc">
            Manage a joint anime list with your partner or friend group.
          </p>
        </bloom-card>

        <bloom-card [hoverable]="false" ariaLabel="Rate & Compare">
          <span class="landing__feature-icon" aria-hidden="true">&#9733;</span>
          <h3 class="landing__feature-title">Rate &amp; Compare</h3>
          <p class="landing__feature-desc">
            See your compatibility score and discover where your tastes diverge.
          </p>
        </bloom-card>

        <bloom-card [hoverable]="false" ariaLabel="Discover Together">
          <span class="landing__feature-icon" aria-hidden="true">&#10047;</span>
          <h3 class="landing__feature-title">Discover Together</h3>
          <p class="landing__feature-desc">
            Random backlog picker breaks decision paralysis instantly.
          </p>
        </bloom-card>
      </section>

      <!-- Footer tagline -->
      <p class="landing__footer">Built for couples &amp; small groups who love anime</p>
    </main>
  `,
})
export class Landing {}
