import { Routes } from '@angular/router';
import { ShellLayout } from './core/layout/shell-layout/shell-layout';
import { MinimalLayout } from './core/layout/minimal-layout/minimal-layout';
import { authGuard } from './core/auth/guards/auth.guard';
import { guestGuard } from './core/auth/guards/guest.guard';
import { environment } from '../environments/environment';

export const routes: Routes = [
  // Public routes (minimal layout — no nav bar)
  {
    path: '',
    component: MinimalLayout,
    children: [
      { path: '', loadComponent: () => import('./features/landing/landing').then(m => m.Landing), pathMatch: 'full' },
      { path: 'login', title: 'Sign In', canActivate: [guestGuard], loadComponent: () => import('./features/auth/login').then(m => m.Login) },
      { path: 'register', title: 'Create Account', canActivate: [guestGuard], loadComponent: () => import('./features/auth/register').then(m => m.Register) },
      { path: 'forgot-password', title: 'Forgot Password', loadComponent: () => import('./features/auth/forgot-password/forgot-password').then(m => m.ForgotPassword) },
      { path: 'reset-password', title: 'Reset Password', loadComponent: () => import('./features/auth/reset-password/reset-password').then(m => m.ResetPassword) },
    ],
  },
  // Authenticated routes (shell layout — with nav bar)
  {
    path: '',
    component: ShellLayout,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      { path: 'home', loadChildren: () => import('./features/home/home.routes').then(m => m.homeRoutes) },
      { path: 'watch-spaces', loadChildren: () => import('./features/watch-spaces/watch-spaces.routes').then(m => m.watchSpacesRoutes) },
      { path: 'settings', loadChildren: () => import('./features/settings/settings.routes').then(m => m.settingsRoutes) },
      {
        path: 'invitations/:token/accept',
        title: 'Invitation',
        loadComponent: () => import('./features/watch-spaces/invitation-response').then(m => m.InvitationResponse),
        data: { action: 'accept' },
      },
      {
        path: 'invitations/:token/decline',
        title: 'Invitation',
        loadComponent: () => import('./features/watch-spaces/invitation-response').then(m => m.InvitationResponse),
        data: { action: 'decline' },
      },
      ...(!environment.production ? [{ path: 'showcase', loadChildren: () => import('./features/showcase/showcase.routes').then(m => m.showcaseRoutes) }] : []),
    ],
  },
];
